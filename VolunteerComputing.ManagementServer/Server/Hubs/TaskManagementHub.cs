using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        readonly Dictionary<string, string> taskServers = new();
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

        public async Task JoinTaskServers(string uid)
        {
            lock (connectedServersLocker)
            {
                connectedServers++;
            }
            var id = Context.ConnectionId;
            Console.WriteLine($"Task server joined with connection id: {id}; id: {uid}");

            var (key, _) = taskServers.FirstOrDefault(kv => kv.Value == uid);
            if (key != null)
                taskServers.Remove(key);

            taskServers[id] = uid;

            await Groups.AddToGroupAsync(id, taskServerId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            if (taskServers.ContainsKey(connectionId))
            {
                Console.WriteLine($"Task {connectionId} server lost connection: {exception?.Message}");
                lock (connectedServersLocker)
                {
                    connectedServers--;
                }
                taskServers.Remove(connectionId);
            }
            else
                Console.WriteLine($"Client {connectionId} lost connection: {exception?.Message}");
        }

        public async Task JoinClients()
        {
            Console.WriteLine($"Client {Context.ConnectionId} joined");
            await Groups.AddToGroupAsync(Context.ConnectionId, clientId);
        }

        public async Task<int> CountPacketsByType(int packetTypeId)
        {
            return await dbContext.Packets.CountAsync(p => p.TypeId == packetTypeId);
        }

        public async Task ReportFinished() //to do - verify it is task server
        {
            lock (finishedLocker)
            {
                finished++;
                Console.WriteLine($"Task server reports finishing work ({finished} out of {connectedServers} reported)");
            }
            if (finished == connectedServers)
            {
                finished = 0;
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
                        var now = DateTime.UtcNow;
                        var result = new Result { FileId = fileId, Project = project, CreatedAt = now, SecondsElapsed = (now - project.StartTime).TotalSeconds };
                        dbContext.Results.Add(result);

                        await Clients.Group(clientId).NewResult(result);
                    });

                await Task.WhenAll(tasks);

                foreach (var packet in packets)
                {
                    ShareAPI.RemoveFromShare(packet.Data);
                }
                dbContext.Packets.RemoveRange(packets);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("Saved");

                await Clients.All.InformFinished();

            }
        }
    }
}
