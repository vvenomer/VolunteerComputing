using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
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
        static int connectedServers = 0;
        static int finished = 0;
        readonly object connectedServersLocker = new();
        readonly object finishedLocker = new();
        public const string taskServerId = "t";
        public const string clientId = "c";

        public TaskManagementHub(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task JoinTaskServers()
        {
            lock (connectedServersLocker)
            {
                connectedServers++;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, taskServerId);
        }

        public async Task JoinClients()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, clientId);
        }

        public async Task<int> CountPacketsByType(int packetTypeId)
        {
            return await dbContext.Packets.CountAsync(p => p.TypeId == packetTypeId);
        }

        public async Task ReportFinished()
        {
            lock (finishedLocker)
            {
                finished++;
                Console.WriteLine($"Task server reports finishing work ({finished} out of {connectedServers} reported)");
            }
            if (finished == connectedServers)
            {
                Console.WriteLine("Saving result");
                var packets = dbContext.Packets.Include(x => x.Type).ThenInclude(p => p.Project).ToList();

                var tasks = packets
                    .GroupBy(p => p.Type.Project)
                    .Select(g => (project: g.Key, data: g.Select(x => ShareAPI.GetTextFromShare(x.Data))))
                    .Select(g => (g.project, result: JsonConvert.SerializeObject(g.data)))
                    .Select(async g =>
                    {
                        var (project, data) = g;
                        var fileId = await ResultsHelper.SaveResult(project.Name, data);
                        var result = new Result { FileId = fileId, Project = project, CreatedAt = DateTime.Now };
                        dbContext.Result.Add(result);

                        await Clients.Group(clientId).NewResult(result);
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
