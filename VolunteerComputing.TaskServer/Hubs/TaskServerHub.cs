using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolunteerComputing.Shared;
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
            var device = dbContext.Devices.FirstOrDefault(d => d.ConnectionId == Context.ConnectionId);
            if (device is not null)
            {
                dbContext.Devices.Remove(device);
                await dbContext.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendResult(byte[] result, int programId, bool cpu, double time, double energy)
        {
            //update device data
            var thisDevice = dbContext.Devices.Include(x => x.DeviceStats).ThenInclude(x => x.ComputeTask).FirstOrDefault(d => d.ConnectionId == Context.ConnectionId);
            if (cpu)
                thisDevice.CpuWorks = false;
            else
                thisDevice.GpuWorks = false;
            //save speed?
            var computeTask = dbContext.ComputeTask
                .Include(x => x.PacketTypes)
                .ThenInclude(x => x.PacketType)
                .FirstOrDefault(t => t.Id == programId);

            var stats = thisDevice.DeviceStats.FirstOrDefault(x => x.ComputeTask == computeTask && x.IsCpu == cpu)
                ?? new DeviceStat(thisDevice, computeTask, cpu);
            stats.AddStat(time, energy);

            if (dbContext.Entry(stats).State == EntityState.Detached)
                dbContext.Add(stats);
            
            //get and save packets from result
            var results = JsonConvert.DeserializeObject<List<List<string>>>(CompressionHelper.Decompress(result));
            var newPackets = computeTask
                .PacketTypes
                .Where(p => !p.IsInput)
                .Select(p => p.PacketType)
                .Zip(results)
                .Select(x => x.Second
                    .Select(d => ShareAPI.SaveTextToShare(d))
                    .Select(data => new Packet { Type = x.First, Data = data }))
                .SelectMany(x => x);
            foreach (var packet in newPackets)
            {
                dbContext.Packets.Add(packet);
            }
            await dbContext.SaveChangesAsync();
        }

        public ProgramData GetProgram(int programId, bool windows, bool cpu)
        {
            var task = dbContext.ComputeTask.FirstOrDefault(t => t.Id == programId);
            var programPath = windows
                ? (cpu
                    ? task.WindowsCpuProgram
                    : task.WindowsGpuProgram)
                : (cpu
                    ? task.LinuxCpuProgram
                    : task.LinuxGpuProgram);
            var program = CompressionHelper.CompressFile(ShareAPI.GetFromShare(programPath));
            return new ProgramData { Program = program, ExeName = task.ExeFilename };
        }

        public async Task<int> SendDeviceData(DeviceData device)
        {
            if (device.Id != 0)
                device = dbContext.Devices.Find(device.Id);

            device.ConnectionId = Context.ConnectionId;
            device.CpuWorks = false;
            device.GpuWorks = false;
            device.TaskServerId = TaskProcessorService.Id;

            if (device.Id == 0)
            {
                dbContext.Devices.Add(device);
            }
            await dbContext.SaveChangesAsync();
            return device.Id;
        }

    }
}
