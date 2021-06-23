﻿using System.Collections.Generic;

namespace VolunteerComputing.Shared.Models
{
    public class DeviceData
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public string TaskServerId { get; set; }
        public bool IsWindows { get; set; } //otherwise Linux
        public bool CpuWorks { get; set; }
        public bool CpuAvailable { get; set; }

        public bool GpuWorks { get; set; }
        public bool GpuAvailable { get; set; }

        public double BaseCpuEnergyConsumption { get; set; }
        public double BaseGpuEnergyConsumption { get; set; }

        public ICollection<DeviceStat> DeviceStats { get; set; }
    }
}
