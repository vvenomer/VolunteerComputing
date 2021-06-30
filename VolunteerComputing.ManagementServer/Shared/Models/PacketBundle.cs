using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class PacketBundle
    {
        public int Id { get; set; }
        public int TimesSent { get; set; }
        public int UntilCheck { get; set; }
        public ComputeTask ComputeTask { get; set; }
        public ICollection<Packet> Packets { get; set; }
        public ICollection<BundleResult> BundleResults { get; set; }
    }
}
