using Microsoft.AspNetCore.SignalR;
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
        private readonly IServiceScopeFactory scopeFactory;
        readonly static Random random = new();
        static double changeToUseNewDevice = 0.5;
        public static bool ShouldStartWork { get; set; }

        public TaskProcessorService(IHubContext<TaskServerHub, ITaskServerHubMessages> taskServerHub, IServiceScopeFactory scopeFactory)
        {
            this.taskServerHub = taskServerHub;
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int i = 0, j = 0, k = 0, l = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    while (!ShouldStartWork)
                    {
                        Console.Clear();
                        //Console.SetCursorPosition(0, 0);
                        Console.WriteLine("Waiting for work" + Dots(ref i, 3));
                        await Task.Delay(2000, stoppingToken);
                    }
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Waiting for work: Done");
                    i = 0;
                    using var scope = scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();

                    IEnumerable<TaskWithPackets> tasksWithPackets = null;
                    while (ShouldStartWork && !stoppingToken.IsCancellationRequested)
                    {
                        Console.SetCursorPosition(0, 1);
                        Console.WriteLine("Finding tasks" + Dots(ref j, 3));
                        var packets = context.Packets.Include(x => x.Type).ThenInclude(p => p.Project).ToList();
                        var tasks = context.ComputeTask.Include(x => x.PacketTypes).ThenInclude(x => x.PacketType);

                        tasksWithPackets = FindComputeTasksAndPackets(packets, tasks);

                        if(!tasksWithPackets.Any())
                        {
                            bool nothingDoesWork = !context.Devices.Any(d => d.CpuWorks || d.GpuWorks);
                            if(nothingDoesWork)
                            {
                                Console.SetCursorPosition(0, 2);
                                Console.WriteLine("Finished " + ++k);
                                //to do: create table with project name and use that name to save results
                                foreach (var grouping in packets.GroupBy(p => p.Type.Project))
                                {
                                    var project = grouping.Key.Name;
                                    File.WriteAllText(Path.GetRandomFileName() + $".{project}_result.json", JsonConvert.SerializeObject(grouping.Select(x => ShareAPI.GetTextFromShare(x.Data))));
                                }
                                
                                context.Packets.RemoveRange(packets);
                                await context.SaveChangesAsync(stoppingToken);
                                ShouldStartWork = false;
                                continue; //continue?
                            }
                            await Task.Delay(2000, stoppingToken); //wait for new devices
                            continue;
                        }
                        break;
                    }

                    IEnumerable<DeviceData> devices = null;
                    while (ShouldStartWork && !stoppingToken.IsCancellationRequested)
                    {
                        Console.SetCursorPosition(0, 2);
                        Console.WriteLine("Finding devices" + Dots(ref l, 3));
                        devices = context.Devices.Include(d => d.DeviceStats).Where(d => (d.CpuAvailable && !d.CpuWorks) || (d.GpuAvailable && !d.GpuWorks));
                        //wait if not found
                        if (!devices.Any())
                        {
                            await Task.Delay(2000, stoppingToken); //wait for new devices
                            continue;
                        }
                        break;
                    }

                    foreach (var taskWithPackets in tasksWithPackets.ToList())
                    {
                        var computeTask = taskWithPackets.ComputeTask;
                        var (selectedDevice, isCpu) = ChooseDevice(devices, computeTask);

                        if(selectedDevice is not null)
                        {
                            context.Packets.RemoveRange(taskWithPackets.PacketsToRemove); //should set as being worked on by device and when device crushes set back or deleted if completes

                            if (isCpu)
                                selectedDevice.CpuWorks = true;
                            else
                                selectedDevice.GpuWorks = true;
                            var dbSaveAwaiter = context.SaveChangesAsync(stoppingToken);
                            var data = JsonConvert.SerializeObject(taskWithPackets.PacketsToSend.Select(p => p.Data).Select(d => ShareAPI.GetTextFromShare(d)));
                            await taskServerHub.Clients.Client(selectedDevice.ConnectionId).SendTaskAsync(computeTask.Id, data, isCpu);
                            await dbSaveAwaiter;

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }


            /*try
            {
                int i = 0, j = 0, k = 0, l = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    while (!ShouldStartWork)
                    {
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("Waiting for work" + Dots(ref i, 3));
                        await Task.Delay(2000, stoppingToken);
                    }
                    Console.SetCursorPosition(0, 1);
                    Console.WriteLine("Finding devices" + Dots(ref j, 3));
                    using var scope = scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<VolunteerComputingContext>();

                    var aviableDevices = context.Devices.Where(d => (d.CpuAvailable && !d.CpuWorks) || (d.GpuAvailable && !d.GpuWorks));

                    var (selectedDevice, isCpu) = ChooseDevice(aviableDevices);

                    if (selectedDevice is null)
                    {
                        await Task.Delay(2000, stoppingToken); //wait for new devices
                        continue;
                    }

                    while (ShouldStartWork && !stoppingToken.IsCancellationRequested)
                    {
                        Console.SetCursorPosition(0, 2);
                        Console.WriteLine("Finding tasks" + Dots(ref k, 3));
                        var packets = context.Packets.Include(x => x.Type).ToList();

                        var (computeTask, selectedPackets, packetsToDelete) = FindComputeTasksAndPackets(packets, context.ComputeTask.Include(x => x.PacketTypes).ThenInclude(x => x.PacketType)).First();
                        if (computeTask is null)
                        {
                            if (aviableDevices.Count() == context.Devices.Count() && packets.Any())
                            {
                                //there are aviable devices, all devices are aviable and there is nothing to do - reset

                                Console.SetCursorPosition(0, 3);
                                Console.WriteLine("Finished " + ++l);
                                //to do: create table with project name and use that name to save results
                                File.WriteAllText(Path.GetRandomFileName() + "_result.json", JsonConvert.SerializeObject(packets.Select(x => ShareAPI.GetTextFromShare(x.Data))));
                                context.Packets.RemoveRange(packets);
                                await context.SaveChangesAsync();
                                ShouldStartWork = false;
                                continue;
                            }
                            await Task.Delay(500, stoppingToken); //can't do any task -> wait for new results
                            continue;
                        }
                        //if eg. isCpu && selectedDevice.IsWindows && computeTask.WindowsCpuProgram is null -> can't do - find new task? new device?
                        context.Packets.RemoveRange(packetsToDelete); //delete files from share too (or wait?)

                        var dbSaveAwaiter = context.SaveChangesAsync(stoppingToken);
                        if (isCpu)
                            selectedDevice.CpuWorks = true;
                        else
                            selectedDevice.GpuWorks = true;
                        var data = JsonConvert.SerializeObject(selectedPackets.Select(p => p.Data).Select(d => ShareAPI.GetTextFromShare(d)));
                        await taskServerHub.Clients.Client(selectedDevice.ConnectionId).SendTaskAsync(computeTask.Id, data, isCpu);
                        await dbSaveAwaiter;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }*/
        }

        static string Dots(ref int numberOfDots, int max)
        {
            numberOfDots++;
            if (numberOfDots > max)
                numberOfDots = 0;
            var i = numberOfDots;
            return new string(Enumerable.Range(0, max).Select(n => n < i ? '.' : ' ').ToArray());
        }

        private static (DeviceData device, bool isCpu) ChooseDevice(IEnumerable<DeviceData> aviableDevices, ComputeTask computeTask)
        {
            List<(DeviceData device, bool isCpu, double speed)> devices = new();

            if(computeTask.WindowsCpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => d.IsWindows && d.CpuAvailable && !d.CpuWorks)
                    .Select(d => (device: d, isCpu: true, speed: d.DeviceStats.Where(s => s.ComputeTask == computeTask && s.IsCpu).Select(s => s.TimeSum / s.Count).FirstOrDefault()));
                devices.AddRange(newDevices);
            }
            if (computeTask.WindowsGpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => d.IsWindows && d.GpuAvailable && !d.GpuWorks)
                    .Select(d => (device: d, isCpu: false, speed: d.DeviceStats.Where(s => s.ComputeTask == computeTask && !s.IsCpu).Select(s => s.TimeSum / s.Count).FirstOrDefault()));
                devices.AddRange(newDevices);
            }
            if (computeTask.LinuxCpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => !d.IsWindows && d.CpuAvailable && !d.CpuWorks)
                    .Select(d => (device: d, isCpu: true, speed: d.DeviceStats.Where(s => s.ComputeTask == computeTask && s.IsCpu).Select(s => s.TimeSum / s.Count).FirstOrDefault()));
                devices.AddRange(newDevices);
            }
            if (computeTask.LinuxGpuProgram is not null)
            {
                var newDevices = aviableDevices
                    .Where(d => !d.IsWindows && d.GpuAvailable && !d.GpuWorks)
                    .Select(d => (device: d, isCpu: false, speed: d.DeviceStats.Where(s => s.ComputeTask == computeTask && !s.IsCpu).Select(s => s.TimeSum / s.Count).FirstOrDefault()));
                devices.AddRange(newDevices);
            }

            (DeviceData device, bool isCpu, double speed) device;
            if (devices.Any(d => d.speed == 0) && devices.Any(d => d.speed > 0) && random.NextDouble() < changeToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                device = devices
                    .FirstOrDefault(d => d.speed == 0);
            }
            else
            {
                device = devices
                    .OrderBy(d => d.speed)
                    .FirstOrDefault();
            }

            return (device.device, device.isCpu);
        }

        private static IEnumerable<TaskWithPackets> FindComputeTasksAndPackets(IEnumerable<Packet> packets, IEnumerable<ComputeTask> computeTasks)
        {
            foreach (var computeTask in computeTasks)
            {
                var tempPackets = packets.ToList();
                var selectednewPackets = new List<Packet>();
                var selectedPackets = new List<Packet>();
                var inputPacketTypes = computeTask.PacketTypes
                    .Where(p => p.IsInput)
                    .OrderBy(p => p.Id)
                    .Select(p => p.PacketType)
                    .ToList();
                foreach (var packetType in inputPacketTypes)
                {

                    if (packetType.Aggregable)
                    {
                        var alreadyAgdregated = tempPackets.FirstOrDefault(t => t.Aggregated);
                        if (alreadyAgdregated is not null)
                        {
                            selectednewPackets.Add(alreadyAgdregated);
                            selectedPackets.Add(alreadyAgdregated);
                            tempPackets.Remove(alreadyAgdregated);
                        }
                        else
                        {
                            var packetsToSelect = tempPackets.Where(t => t.Type == packetType).ToHashSet();
                            if (packetsToSelect.Count < 2)
                                break; //not enough to aggregate
                            var data = packetsToSelect
                                .Select(p => $"{ShareAPI.GetTextFromShare(p.Data)}")
                                .Aggregate((a, b) => $"{a},{b}");
                            selectedPackets.AddRange(packetsToSelect);
                            selectednewPackets.Add(new Packet
                            {
                                Type = packetType,
                                Data = ShareAPI.SaveTextToShare($"[{data}]"),
                                Aggregated = true
                            });
                            tempPackets.RemoveAll(x => packetsToSelect.Contains(x));
                        }
                    }
                    else
                    {
                        var packetToSelect = tempPackets.FirstOrDefault(t => t.Type == packetType);
                        if (packetToSelect == null)
                            break;
                        selectednewPackets.Add(packetToSelect);
                        selectedPackets.Add(packetToSelect);
                        tempPackets.Remove(packetToSelect);
                    }
                }
                if (selectednewPackets.Count == inputPacketTypes.Count)
                    yield return new TaskWithPackets { ComputeTask = computeTask, PacketsToSend = selectednewPackets, PacketsToRemove = selectedPackets };
            }
        }

        class TaskWithPackets
        {
            public ComputeTask ComputeTask { get; set; }
            public List<Packet> PacketsToSend { get; set; }
            public List<Packet> PacketsToRemove { get; set; }
        }
    }
}
