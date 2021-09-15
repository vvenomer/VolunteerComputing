using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Models;
using VolunteerComputing.TaskServer.Data;
using VolunteerComputing.TaskServer.Hubs;

namespace VolunteerComputing.TaskServer.Services
{
    public class TaskProcessorService : BackgroundService
    {
        readonly IHubContext<TaskServerHub, ITaskServerHubMessages> taskServerHub;
        readonly IServiceScopeFactory scopeFactory;
        readonly HubConnection hubConnection;
        public static bool ShouldStartWork { get; set; }
        public static string Id { get; set; }

        public TaskProcessorService(IHubContext<TaskServerHub, ITaskServerHubMessages> taskServerHub, IServiceScopeFactory scopeFactory)
        {
            this.taskServerHub = taskServerHub;
            this.scopeFactory = scopeFactory;

            Id = Guid.NewGuid().ToString();

            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/tasks")
                .AddMessagePackProtocol()
                .WithAutomaticReconnect()
                .Build();
            hubConnection.Closed += (ex) =>
            {
                Console.WriteLine($"Connection closed. Error: {ex.Message}");
                return Task.CompletedTask;
            };
            hubConnection.Reconnecting += (ex) =>
            {
                Console.WriteLine($"Connection reconnecting. Error: {ex.Message}");
                return Task.CompletedTask;
            };
            hubConnection.Reconnected += async (message) =>
            {
                Console.WriteLine($"Connection reconnected. Message: {message}");
                await hubConnection.InvokeAsync("JoinTaskServers", Id);
            };
            hubConnection.On("PacketAdded", () => ShouldStartWork = true);
            hubConnection.On("InformFinished", async () =>
            {
                Console.WriteLine("Finished work");
                await taskServerHub.Clients.All.InformFinished();
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    await hubConnection.StartAsync(stoppingToken);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(1000, stoppingToken);
                }
                if (stoppingToken.IsCancellationRequested)
                    return;
            } while (true);
            Console.WriteLine("Connected");
            await hubConnection.InvokeAsync("JoinTaskServers", Id, cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WaitForWork(stoppingToken);

                    var devices = await FindAvailableDevices(stoppingToken);

                    var bundles = await CreateAndFindBundlesLoop(devices, stoppingToken);
                    if (bundles is null)
                        continue;

                    using var scope = scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();
                    await DeviceManager.RefreshDevices(devices, context);
                    foreach (var bundle in bundles)
                    {
                        var computeTask = bundle.ComputeTask;

                        var grouped = bundle.Packets
                            .GroupBy(p => p.Type)
                            .Select(p => p.Count() > 1 ? PacketManager.AggregatePackets(p.ToList()) : p.FirstOrDefault())
                            .Select(p => p.Aggregated ? p.Data : ShareAPI.GetTextFromShare(p.Data))
                            .ToList();

                        //device shouldn't work on same packet twice
                        var devicesForBundle = devices
                            .Except(bundle.BundleResults
                                .SelectMany(r => r.Packets)
                                .Select(p => p.DeviceWorkedOnIt));

                        for (int i = bundle.TimesSent; i < computeTask.Project.MinAgreeingClients; i++)
                        {
                            var selectedDeviceWithStat = DeviceManager.ChooseDevice(devicesForBundle, computeTask);
                            var selectedDevice = selectedDeviceWithStat?.Device;

                            if (selectedDevice is not null)
                            {
                                bundle.TimesSent++;

                                var isCpu = selectedDeviceWithStat.IsCpu;
                                if (isCpu)
                                    selectedDevice.CpuWorksOnBundle = bundle.Id;
                                else
                                    selectedDevice.GpuWorksOnBundle = bundle.Id;

                                context.Entry(bundle).State = EntityState.Modified;

                                var dbSaveAwaiter = context.SaveChangesAsync(stoppingToken);
                                var data = CompressionHelper.Compress(JsonConvert.SerializeObject(grouped));
                                await taskServerHub.Clients.Client(selectedDevice.ConnectionId).SendTaskAsync(computeTask.Id, data, isCpu);
                                await dbSaveAwaiter;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        async Task<IEnumerable<PacketBundle>> CreateAndFindBundlesLoop(IEnumerable<DeviceData> devices, CancellationToken stoppingToken)
        {
            int j = 0, k = 0;
            while (ShouldStartWork && !stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();
                Console.SetCursorPosition(0, 1);
                Console.WriteLine("Finding tasks" + Dots(ref j, 3));
                var bundles = await PacketManager.CreateAndFindBundles(devices, context);

                if (!bundles.Any())
                {
                    bool noWorkTodo = !context.Bundles.Any();
                    if (noWorkTodo)
                    {
                        Console.SetCursorPosition(0, 4);
                        Console.WriteLine("Finished " + ++k);

                        await hubConnection.SendAsync("ReportFinished", cancellationToken: stoppingToken);
                        ShouldStartWork = false;
                        return null;
                    }
                    await Task.Delay(2000, stoppingToken); //wait for new devices
                    continue;
                }

                return bundles;
            }

            return null;
        }

        static async Task WaitForWork(CancellationToken stoppingToken)
        {
            int i = 0;
            while (!ShouldStartWork)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Waiting for work" + Dots(ref i, 3));
                await Task.Delay(2000, stoppingToken);
            }
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Waiting for work: Done");
        }

        async Task<IEnumerable<DeviceData>> FindAvailableDevices(CancellationToken stoppingToken)
        {
            int l = 0;
            IEnumerable<DeviceData> devices = null;
            while (ShouldStartWork && !stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();

                Console.SetCursorPosition(0, 2);
                Console.WriteLine("Finding devices" + Dots(ref l, 3));
                devices = context.Devices
                    .Include(d => d.DeviceStats)
                    .Where(d => d.TaskServerId == Id)
                    .AsEnumerable()
                    .Where(d => d.IsAvailable)
                    .ToList();
                //wait if not found
                if (!devices.Any())
                {
                    await Task.Delay(2000, stoppingToken); //wait for new devices
                    continue;
                }
                break;
            }
            return devices;
        }

        static string Dots(ref int numberOfDots, int max)
        {
            numberOfDots++;
            if (numberOfDots > max)
                numberOfDots = 0;
            var i = numberOfDots;
            return new string(Enumerable.Range(0, max).Select(n => n < i ? '.' : ' ').ToArray());
        }
    }
}
