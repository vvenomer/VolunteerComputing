using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.ManagementServer.Server.Data;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Hubs
{
    public class TaskManagementHub : Hub<ITaskManagementHubMessages>
    {
        private readonly ApplicationDbContext dbContext;
        static int connected = 0;
        static int finished = 0;
        object connectedLocker = new();
        object finishedLocker = new();

        public TaskManagementHub(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override Task OnConnectedAsync()
        {
            lock (connectedLocker)
            {
                connected++;
            }
            return base.OnConnectedAsync();
        }

        public async Task ReportFinished()
        {
            lock (finishedLocker)
            {
                finished++;
                Console.WriteLine($"Task server reports finishing work ({finished} out of {connected} reported)");
            }
            if (finished == connected)
            {
                Console.WriteLine("Saving result");
                var packets = dbContext.Packets.Include(x => x.Type).ThenInclude(p => p.Project).ToList();

                var tasks = packets
                    .GroupBy(p => p.Type.Project)
                    .Select(g => (project: g.Key, data: g.Select(x => ShareAPI.GetTextFromShare(x.Data))))
                    .Select(g => (g.project, result: JsonConvert.SerializeObject(g.data)))
                    .Select(async g =>
                    {
                        var (project, result) = g;
                        var fileId = await ResultsHelper.SaveResult(project.Name, result);
                        dbContext.Result.Add(new Result { FileId = fileId, Project = project });
                    });

                await Task.WhenAll(tasks);

                dbContext.Packets.RemoveRange(packets); //todo: delete from share
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Saved");

                await Clients.All.InformFinished();

            }
        }
    }
}
