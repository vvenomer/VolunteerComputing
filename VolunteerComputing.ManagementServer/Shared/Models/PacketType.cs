using System;
using System.Collections.Generic;

namespace VolunteerComputing.Shared.Models
{
    public class PacketType
    {
        public int Id { get; set; }
        public string Type { get; set; } //np. state, number, steps
        public bool Aggregable { get; set; } //in case of collaz.steps;

        public Project Project { get; set; } //collaz

        public ICollection<PacketTypeToComputeTask> ComputeTasks { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PacketType type &&
                   Id == type.Id &&
                   Type == type.Type &&
                   Project == type.Project;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Type, Project);
        }
    }
}
