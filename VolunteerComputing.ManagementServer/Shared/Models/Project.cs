using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinAgreeingClients { get; set; }
        public ICollection<ComputeTask> ComputeTasks { get; set; }
        public ICollection<PacketType> PacketTypes { get; set; }
        public ICollection<Result> Results { get; set; }
    }
}
