using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VolunteerComputing.ManagementServer.Server.Hubs
{
    public interface ITaskManagementHubMessages
    {
        public Task InformFinished();
        public Task PacketAdded();
    }
}
