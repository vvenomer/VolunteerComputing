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

namespace VolunteerComputing.ManagementServer.Server.Hubs
{
    public class TaskManagementHub : Hub
    {
        private readonly ApplicationDbContext dbContext;
        int connected = 0;
        int finished = 0;
        object connectedLocker = new();
        object finishedLocker = new();

        public TaskManagementHub(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override Task OnConnectedAsync()
        {
            lock(connectedLocker)
            {
                connected++;
            }
            return base.OnConnectedAsync();
        }

        public async Task ReportFinished()
        {
            lock(finishedLocker)
            {
                Console.WriteLine($"Task server reports finishing work ({finished} out of {connected} reported)");
                finished++;
            }
            if(finished == connected)
            {
                _ = Task.Run(async () =>
                {
                    Console.WriteLine("Saving result");
                    var packets = dbContext.Packets.Include(x => x.Type).ThenInclude(p => p.Project).ToList();

                    foreach (var grouping in packets.GroupBy(p => p.Type.Project))
                    {
                        var project = grouping.Key.Name;
                        var result = JsonConvert.SerializeObject(grouping.Select(x => ShareAPI.GetTextFromShare(x.Data)));
                        File.WriteAllText(Path.GetRandomFileName() + $".{project}_result.json", result);
                    }

                    dbContext.Packets.RemoveRange(packets);
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine("Saved");
                    //inform all clients
                });

            }
        }
    }
}
