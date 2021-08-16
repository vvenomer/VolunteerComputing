using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerComputing.Shared.Models
{
    public class ComputeTask
    {
        public int Id { get; set; }

        public ICollection<PacketTypeToComputeTask> PacketTypes { get; set; }
        public ICollection<DeviceStat> DeviceStats { get; set; }
        public string WindowsCpuProgram { get; set; }
        public string WindowsGpuProgram { get; set; }

        public string LinuxCpuProgram { get; set; }
        public string LinuxGpuProgram { get; set; }
        public string ExeFilename { get; set; }
        public Project Project { get; set; }

        public IEnumerable<(bool isWindows, bool isCpu)> GetAvailablePlatforms()
        {

            if (WindowsCpuProgram is not null)
            {
                yield return (true, true);
            }
            if (WindowsGpuProgram is not null)
            {
                yield return (true, false);
            }
            if (LinuxCpuProgram is not null)
            {
                yield return (false, true);
            }
            if (LinuxGpuProgram is not null)
            {
                yield return (false, false);
            }
        }
    }
}
