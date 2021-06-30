using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<Packet> Extend(IEnumerable<Packet> packets)
        {
            var aggregablePacketTypes = ComputeTask.PacketTypes
                .Where(x => x.IsInput)
                .Select(x => x.PacketType)
                .Where(x => x.Aggregable)
                .ToHashSet();
            var packetsToAdd = packets
                .Where(p => aggregablePacketTypes.Contains(p.Type))
                .ToList();

            foreach (var packet in packetsToAdd)
            {
                Packets.Add(packet);
            }
            return packetsToAdd;
        }
    }
}
