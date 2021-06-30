using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        readonly static Random random = new();
        readonly HubConnection hubConnection;
        static double changeToUseNewDevice = 0.5;
        public static bool ShouldStartWork { get; set; }
        public static string Id { get; set; }

        public TaskProcessorService(IHubContext<TaskServerHub, ITaskServerHubMessages> taskServerHub, IServiceScopeFactory scopeFactory)
        {
            this.taskServerHub = taskServerHub;
            this.scopeFactory = scopeFactory;

            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/tasks")
                .AddMessagePackProtocol()
                .WithAutomaticReconnect()
                .Build();
            hubConnection.On("PacketAdded", () => ShouldStartWork = true);
            hubConnection.On("InformFinished", async () =>
            {
                Console.WriteLine("Finished work");
                await taskServerHub.Clients.All.InformFinished();
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await hubConnection.StartAsync(stoppingToken);
            Id = hubConnection.ConnectionId;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WaitForWork(stoppingToken);

                    var devices = await FindAvailableDevices(stoppingToken);

                    var bundles = await CreateAndFindBundles(stoppingToken, devices);

                    using var scope = scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();
                    await RefreshDevices(devices, context);
                    foreach (var bundle in bundles)
                    {
                        var computeTask = bundle.ComputeTask;

                        var grouped = bundle.Packets
                            .GroupBy(p => p.Type)
                            .Select(p => p.Count() > 1 ? AggregatePackets(p.ToList()) : p.FirstOrDefault())
                            .Select(p => p.Aggregated ? p.Data : ShareAPI.GetTextFromShare(p.Data))
                            .ToList();

                        for (int i = bundle.TimesSent; i < computeTask.Project.MinAgreeingClients; i++)
                        {
                            var selectedDeviceWithStat = ChooseDevice(devices, computeTask);
                            var selectedDevice = selectedDeviceWithStat?.Device;
                            var isCpu = selectedDeviceWithStat?.IsCpu??false;

                            if (selectedDevice is not null)
                            {
                                bundle.TimesSent++;
                                //for errors:
                                //should set as being worked on by device and when device crushes set back or deleted if completes

                                //for redundancy:
                                //keep it until TimesCalculated != MinAgreeingClients

                                //old
                                //context.Packets.RemoveRange(taskWithPackets.Packets);

                                if (isCpu)
                                    selectedDevice.CpuWorksOnBundle = bundle.Id;
                                else
                                    selectedDevice.GpuWorksOnBundle = bundle.Id;

                                context.Entry(bundle).State = EntityState.Modified;
                                //context.Entry(selectedDevice).State = EntityState.Modified;

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

        Packet AggregatePackets(IList<Packet> packets)
        {
            var data = packets
                .Select(p => ShareAPI.GetTextFromShare(p.Data))
                .Aggregate((a, b) => $"{a},{b}");

            return new Packet
            {
                Type = packets.FirstOrDefault().Type,
                Data = $"[{data}]",
                Aggregated = true
            };
        }

        async Task<IEnumerable<PacketBundle>> CreateAndFindBundles(CancellationToken stoppingToken, IEnumerable<DeviceData> devices)
        {
            //to rework, with bundles for redundant calculations...
            int j = 0, k = 0;
            List<PacketBundle> bundles = new();
            while (ShouldStartWork && !stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();
                Console.SetCursorPosition(0, 1);
                Console.WriteLine("Finding tasks" + Dots(ref j, 3));
                var newPackets = context.Packets
                    .Include(x => x.Type).ThenInclude(p => p.Project)
                    .Where(x => x.BundleId == null && x.BundleResultId == null)
                    .ToList();
                var tasks = context.ComputeTask
                    .Include(x => x.Project)
                    .Include(x => x.PacketTypes).ThenInclude(x => x.PacketType)
                    .ToList();

                newPackets.AddRange(await FindFinishedBundles(context));

                var existingBundles = context.Bundles
                    .Include(x => x.Packets)
                    .Include(x => x.ComputeTask).ThenInclude(x => x.Project)
                    .Where(x => x.TimesSent < x.ComputeTask.Project.MinAgreeingClients)
                    .ToList();
                //to do: extend aggregable bundles that weren't started

                foreach (var bundle in existingBundles)
                {
                    if (bundle.TimesSent != 0)
                        continue;
                    var extendedPackets = bundle.Extend(newPackets);
                    foreach (var packet in extendedPackets)
                    {
                        newPackets.Remove(packet);
                    }
                }

                var newBundles = CreateBundles(newPackets, tasks, devices).ToList();

                foreach (var bundle in newBundles)
                {
                    context.Bundles.Add(bundle);
                }
                await context.SaveChangesAsync();
                bundles.AddRange(existingBundles);
                bundles.AddRange(newBundles);

                if (!bundles.Any())
                {
                    bool nothingDoesWork = !devices.Any(d => d.TaskServerId == Id && (d.CpuWorksOnBundle != 0 || d.GpuWorksOnBundle != 0));
                    if (nothingDoesWork)
                    {
                        Console.SetCursorPosition(0, 2);
                        Console.WriteLine("Finished " + ++k);

                        await hubConnection.SendAsync("ReportFinished", cancellationToken: stoppingToken);

                        ShouldStartWork = false;
                        continue; //continue?
                    }
                    await RefreshDevices(devices, context);
                    await Task.Delay(2000, stoppingToken); //wait for new devices
                    continue;
                }
                break;
            }

            return bundles;
        }

        private static async Task RefreshDevices(IEnumerable<DeviceData> devices, VolunteerComputingContext context)
        {
            await Task.WhenAll(devices.Select(async d => await context.Entry(d).ReloadAsync())); //refresh devices
        }

        private async Task<IEnumerable<Packet>> FindFinishedBundles(VolunteerComputingContext context)
        {
            List<Packet> packets = new();
            var bundleReults = context.BundleResults
                .AsSplitQuery()
                .Include(x => x.Packets)
                .Include(x => x.Bundle)
                    .ThenInclude(x => x.ComputeTask).ThenInclude(x => x.Project)
                .Include(x => x.Bundle.Packets)
                .AsEnumerable()
                .GroupBy(x => x.Bundle);
            foreach (var result in bundleReults)
            {
                var bundle = result.Key;
                if (bundle.UntilCheck > 0)
                    continue;
                var min = result.Key.ComputeTask.Project.MinAgreeingClients;
                var mostAgreedResult = result
                    .GroupBy(x => x.DataHash)
                    .OrderByDescending(x => x.Count())
                    .FirstOrDefault()
                    .ToList();
                if(mostAgreedResult.Count < min)
                {
                    bundle.UntilCheck++;
                    continue;
                }
                var toKeep = mostAgreedResult.FirstOrDefault();
                var toRemove = result
                    .Where(x => x.Id != toKeep.Id);

                context.Packets.RemoveRange(toRemove.SelectMany(x => x.Packets));
                foreach (var packet in toKeep.Packets)
                {
                    packet.BundleId = null;
                    packet.Bundle = null;
                    packet.BundleResultId = null;
                    packet.BundleResult = null;
                    packet.DeviceWorkedOnIt = null;
                }
                context.BundleResults.RemoveRange(result);

                context.Packets.RemoveRange(bundle.Packets);
                context.Bundles.Remove(bundle);

                packets.AddRange(toKeep.Packets);
            }
            await context.SaveChangesAsync();
            return packets;
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

        static DeviceWithStat ChooseDevice(IEnumerable<DeviceData> aviableDevices, ComputeTask computeTask)
        {
            List<DeviceWithStat> devices = new();

            if(computeTask.WindowsCpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => d.IsWindows && d.CpuAvailable && d.CpuWorksOnBundle == 0)
                    .Select(d => new DeviceWithStat(d, true, d.DeviceStats.FirstOrDefault(s => s.ComputeTask == computeTask && s.IsCpu)));
                devices.AddRange(newDevices);
            }
            if (computeTask.WindowsGpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => d.IsWindows && d.GpuAvailable && d.GpuWorksOnBundle == 0)
                    .Select(d => new DeviceWithStat(d, false, d.DeviceStats.FirstOrDefault(s => s.ComputeTask == computeTask && !s.IsCpu)));
                devices.AddRange(newDevices);
            }
            if (computeTask.LinuxCpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => !d.IsWindows && d.CpuAvailable && d.CpuWorksOnBundle == 0)
                    .Select(d => new DeviceWithStat(d, true, d.DeviceStats.FirstOrDefault(s => s.ComputeTask == computeTask && s.IsCpu)));
                devices.AddRange(newDevices);
            }
            if (computeTask.LinuxGpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => !d.IsWindows && d.GpuAvailable && d.GpuWorksOnBundle == 0)
                    .Select(d => new DeviceWithStat(d, false, d.DeviceStats.FirstOrDefault(s => s.ComputeTask == computeTask && !s.IsCpu)));
                devices.AddRange(newDevices);
            }

            DeviceWithStat device;

            if (devices.Any(d => d.EnergyEfficiency == 0) && devices.Any(d => d.EnergyEfficiency > 0) && random.NextDouble() < changeToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                device = devices
                    .FirstOrDefault(d => d.EnergyEfficiency == 0);
            }
            else
            {
                //special case for when there are no devices?
                device = devices
                    .OrderByDescending(d => d.EnergyEfficiency)
                    .FirstOrDefault();
            }

            return device;
        }

        static IEnumerable<PacketBundle> CreateBundles(List<Packet> packets, IEnumerable<ComputeTask> computeTasks, IEnumerable<DeviceData> devices)
        {
            foreach (var computeTask in computeTasks)
            {
                if (!devices.Any(d => d.CanCalculate(computeTask)))
                    break;

                while(true)
                {
                    var selectedPackets = new List<Packet>();
                    var inputPacketTypes = computeTask.PacketTypes
                        .Where(p => p.IsInput)
                        .OrderBy(p => p.Id)
                        .Select(p => p.PacketType)
                        .ToList();
                    var addedCount = 0;
                    foreach (var packetType in inputPacketTypes)
                    {
                        if (packetType.Aggregable)
                        {
                            var packetsToSelect = packets.Where(t => t.Type == packetType).ToHashSet();
                            if (packetsToSelect.Count < 2)
                                break;
                            selectedPackets.AddRange(packetsToSelect);
                            packets.RemoveAll((Packet x) => packetsToSelect.Contains(x));
                        }
                        else
                        {
                            var packetToSelect = packets.FirstOrDefault(t => t.Type == packetType);
                            if (packetToSelect == null)
                                break;
                            selectedPackets.Add(packetToSelect);
                            packets.Remove(packetToSelect);
                        }
                        addedCount++;
                    }
                    if (addedCount == inputPacketTypes.Count)
                        yield return new PacketBundle { ComputeTask = computeTask, Packets = selectedPackets, UntilCheck = computeTask.Project.MinAgreeingClients };
                    else
                        break;
                    continue;
                }
            }
        }

        class DeviceWithStat
        {
            double time;
            double energy;
            public DeviceWithStat(DeviceData deviceData, bool isCpu, DeviceStat stat)
            {
                Device = deviceData;
                IsCpu = isCpu;
                if(stat is null || stat.Count == 0)
                {
                    time = energy = double.PositiveInfinity;
                }
                else
                {
                    var baseConsumption = (stat.IsCpu ? Device.BaseCpuEnergyConsumption : Device.BaseGpuEnergyConsumption);

                    time = stat.TimeSum / stat.Count;
                    energy = stat.EnergySum / stat.Count - baseConsumption;
                }
                SpeedEfficiency = 1 / time;
                EnergyEfficiency = 1 / time / energy;
                EnergyWithoutTime = 1 / time;
            }

            public DeviceData Device { get; }
            public bool IsCpu { get; }
            public double SpeedEfficiency { get; }
            public double EnergyEfficiency { get; }
            public double EnergyWithoutTime { get; }
        }
    }
}
