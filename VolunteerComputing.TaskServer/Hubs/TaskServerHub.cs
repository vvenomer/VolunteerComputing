using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Dto;
using VolunteerComputing.Shared.Models;
using VolunteerComputing.TaskServer.Data;
using VolunteerComputing.TaskServer.Services;

namespace VolunteerComputing.TaskServer.Hubs
{
    public class TaskServerHub : Hub<ITaskServerHubMessages>
    {
        private readonly VolunteerComputingContext dbContext;
        public TaskServerHub(VolunteerComputingContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine(exception);
            var device = GetDevice();
            if (device is not null)
            {
                await HandleFailedCalculationAsync(device, true, saveChanges: false);
                await HandleFailedCalculationAsync(device, false, saveChanges: false);
                device.TaskServerId = null;
                device.ConnectionId = null;
                await dbContext.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendResult(byte[] result, int programId, bool cpu, double time, double energy)
        {
            //update device data
            var device = GetDevice(d => d.Include(x => x.DeviceStats).ThenInclude(x => x.ComputeTask));

            var bundle = dbContext.Bundles.Find(cpu ? device.CpuWorksOnBundle : device.GpuWorksOnBundle);

            if (cpu)
                device.CpuWorksOnBundle = 0;
            else
                device.GpuWorksOnBundle = 0;
            await dbContext.SaveChangesAsync();

            if(bundle is null)
            {
                Console.WriteLine("Error, bundle doesn't exist");
                return;
            }

            var computeTask = dbContext.ComputeTasks
                .Include(x => x.PacketTypes)
                .ThenInclude(x => x.PacketType)
                .FirstOrDefault(t => t.Id == programId);

            var stats = device.DeviceStats.FirstOrDefault(x => x.ComputeTask == computeTask && x.IsCpu == cpu)
                ?? new DeviceStat(device, computeTask, cpu);
            stats.AddStat(time, energy);

            if (dbContext.Entry(stats).State == EntityState.Detached)
                dbContext.Add(stats);

            var decompressed = CompressionHelper.DecompressData(result);

            var bundleResult = new BundleResult { Bundle = bundle }
                .SetDataHash(decompressed);
            //get and save packets from result
            var results = JsonConvert.DeserializeObject<List<List<string>>>(Encoding.Unicode.GetString(decompressed));
            var newPackets = computeTask
                .PacketTypes
                .Where(p => !p.IsInput)
                .Select(p => p.PacketType)
                .Zip(results)
                .Select(x => x.Second
                    .Select(d => ShareAPI.SaveTextToShare(d))
                    .Select(data => new Packet { Type = x.First, Data = data, DeviceWorkedOnIt = device, BundleResult = bundleResult }))
                .SelectMany(x => x);
            foreach (var packet in newPackets)
            {
                dbContext.Packets.Add(packet);
            }
            dbContext.BundleResults.Add(bundleResult);

            await dbContext.SaveChangesAsync();
        }

        public ProgramData GetProgram(int programId, bool windows, bool cpu)
        {
            var task = dbContext.ComputeTasks.FirstOrDefault(t => t.Id == programId);
            var programPath = windows
                ? (cpu
                    ? task.WindowsCpuProgram
                    : task.WindowsGpuProgram)
                : (cpu
                    ? task.LinuxCpuProgram
                    : task.LinuxGpuProgram);
            var program = CompressionHelper.CompressData(ShareAPI.GetFromShare(programPath));
            return new ProgramData { Program = program, ExeName = task.ExeFilename };
        }

        public async Task<int> SendDeviceData(DeviceData device)
        {
            if (device.Id != 0)
            {
                var dbdevice = dbContext.Devices.Find(device.Id);
                if (device.BaseCpuEnergyConsumption != -1)
                    dbdevice.BaseCpuEnergyConsumption = device.BaseCpuEnergyConsumption;
                if (device.BaseGpuEnergyConsumption != -1)
                    dbdevice.BaseGpuEnergyConsumption = device.BaseGpuEnergyConsumption;
                dbdevice.CpuAvailable = device.CpuAvailable;
                dbdevice.GpuAvailable = device.GpuAvailable;
                device = dbdevice;
            }

            if (device == null)
                return -1;

            device.ConnectionId = Context.ConnectionId;
            device.CpuWorksOnBundle = 0;
            device.GpuWorksOnBundle = 0;
            while (TaskProcessorService.Id == null)
                await Task.Delay(100);
            device.TaskServerId = TaskProcessorService.Id;

            if (device.Id == 0)
                dbContext.Devices.Add(device);

            await dbContext.SaveChangesAsync();
            return device.Id;
        }

        public async Task CalculationsFailed(bool isCpu)
        {
            var device = GetDevice();
            Console.WriteLine($"Calculations failed on {device?.Id} device with connection id: {Context.ConnectionId}");
            await HandleFailedCalculationAsync(device, isCpu);
        }

        async Task HandleFailedCalculationAsync(DeviceData device, bool isCpu, bool saveChanges = true)
        {
            var bundleId = isCpu ? device.CpuWorksOnBundle : device.GpuWorksOnBundle;
            if (bundleId == 0)
                return;
            var bundle = await dbContext.Bundles.FindAsync(bundleId);
            if (bundle is not null)
                bundle.TimesSent--;

            if (isCpu)
                device.CpuWorksOnBundle = 0;
            else
                device.GpuWorksOnBundle = 0;
            if (saveChanges)
                await dbContext.SaveChangesAsync();
        }

        DeviceData GetDevice(Func<IQueryable<DeviceData>, IQueryable<DeviceData>> includes = null)
        {
            if (includes != null)
                return includes(dbContext.Devices).FirstOrDefault(d => d.ConnectionId == Context.ConnectionId);

            return dbContext.Devices.FirstOrDefault(d => d.ConnectionId == Context.ConnectionId);
        }
    }
}
