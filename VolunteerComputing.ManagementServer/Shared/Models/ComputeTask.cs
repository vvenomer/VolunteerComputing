using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VolunteerComputing.Shared.Models
{
    public class ComputeTask
    {
        public int Id { get; set; }

        public IList<PacketTypeToComputeTask> PacketTypes { get; set; }
        public string WindowsCpuProgram { get; set; }
        public string WindowsGpuProgram { get; set; }

        public string LinuxCpuProgram { get; set; }
        public string LinuxGpuProgram { get; set; }
        public string ExeFilename { get; set; }
    }
}
