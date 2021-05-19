using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class PacketTypeToComputeTask
    {
        public int Id { get; set; }
        public PacketType PacketType { get; set; }
        public ComputeTask ComputeTask { get; set; }
        public bool IsInput { get; set; }
    }
}
