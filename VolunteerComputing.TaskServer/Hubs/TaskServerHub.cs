using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Models;
using VolunteerComputing.TaskServer.Data;

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
            if(device is not null)
            {
                dbContext.Devices.Remove(device);
                await dbContext.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendResult(string jsonResult, int programId, bool cpu)
        {
            var thisDevice = dbContext.Devices.FirstOrDefault(d => d.ConnectionId == Context.ConnectionId);
            if (cpu)
                thisDevice.CpuWorks = false;
            else
                thisDevice.GpuWorks = false;
            var outputTypes = dbContext.ComputeTask
                .Include(x => x.PacketTypes)
                .ThenInclude(x => x.PacketType)
                .FirstOrDefault(t => t.Id == programId)
                .PacketTypes
                .Where(p => !p.IsInput)
                .Select(p => p.PacketType);
            var results = JsonConvert.DeserializeObject<List<List<string>>>(jsonResult);
            var newPackets = outputTypes
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
            string program = ShareAPI.GetFromShare(programPath);
            return new ProgramData { Program = program, ExeName = task.ExeFilename };
        }

        public async Task SendDeviceData(bool isWindows, bool hasCpu, bool hasGpu /*... energy, speed ...*/)
        {
            //if doesn't have gpu, it might as well be always working
            dbContext.Devices.Add(new DeviceData()
            {
                ConnectionId = Context.ConnectionId,
                CpuAvailable = hasCpu,
                GpuAvailable = hasGpu,
                IsWindows = isWindows
            });
            await dbContext.SaveChangesAsync();
            //...
        }

    }
}
