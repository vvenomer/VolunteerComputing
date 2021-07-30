using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.ManagementServer.Server.Hubs
{
    public interface ITaskManagementHubMessages
    {
        public Task InformFinished();
        public Task PacketAdded();
        public Task NewResult(Result result);
    }
}
