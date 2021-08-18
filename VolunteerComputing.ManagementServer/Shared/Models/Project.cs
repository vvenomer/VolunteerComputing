using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using VolunteerComputing.Shared.Dto;

namespace VolunteerComputing.Shared.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinAgreeingClients { get; set; }
        public ChoosingStrategy ChoosingStrategy { get; set; }
        public double ChanceToUseNewDevice { get; set; }
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

        [NotMapped]
        [JsonIgnore]
        public Strategy Strategy
        {
            get
            {
                return new Strategy() { ChoosingStrategy = ChoosingStrategy, ChanceToUseNewDevice = ChanceToUseNewDevice };
            }
            set
            {
                (ChoosingStrategy, ChanceToUseNewDevice) = (value.ChoosingStrategy, value.ChanceToUseNewDevice);
            }
        }
    }
}
