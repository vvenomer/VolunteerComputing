using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using VolunteerComputing.Shared.Models;
using VolunteerComputing.TaskServer.Services;

namespace VolunteerComputing.TaskServer.Hubs
{
    public class TaskRecieverHub : Hub
    {

        public void PacketAdded()
        {
            TaskProcessorService.ShouldStartWork = true;
        }
    }
}
