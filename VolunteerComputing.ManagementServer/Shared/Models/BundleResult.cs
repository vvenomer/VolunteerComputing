using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class BundleResult
    {
        public int Id { get; set; }
        public byte[] DataHash { get; set; }

        public PacketBundle Bundle { get; set; }

        public ICollection<Packet> Packets { get; set; }
    }
}
