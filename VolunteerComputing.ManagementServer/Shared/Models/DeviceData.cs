using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VolunteerComputing.Shared.Models
{
    public class DeviceData
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public string TaskServerId { get; set; }
        public bool IsWindows { get; set; } //otherwise Linux
        public int CpuWorksOnBundle { get; set; }
        public bool CpuAvailable { get; set; }

        public int GpuWorksOnBundle { get; set; }
        public bool GpuAvailable { get; set; }

        public double BaseCpuEnergyConsumption { get; set; }
        public double BaseGpuEnergyConsumption { get; set; }

        public ICollection<DeviceStat> DeviceStats { get; set; }

        [JsonIgnore]
        public bool IsAvailable => (CpuAvailable && CpuWorksOnBundle == 0) || (GpuAvailable && GpuWorksOnBundle == 0);

        public bool CanCalculate(ComputeTask computeTask)
        {
            return
                (computeTask.WindowsCpuProgram is not null && IsWindows && CpuAvailable && CpuWorksOnBundle == 0)
                ||
                (computeTask.WindowsGpuProgram is not null && IsWindows && GpuAvailable && GpuWorksOnBundle == 0)
                ||
                (computeTask.LinuxCpuProgram is not null && !IsWindows && CpuAvailable && CpuWorksOnBundle == 0)
                ||
                (computeTask.LinuxGpuProgram is not null && !IsWindows && GpuAvailable && GpuWorksOnBundle == 0);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
