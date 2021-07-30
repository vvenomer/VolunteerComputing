using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            return obj is Project project &&
                   Id == project.Id &&
                   Name == project.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }
}
