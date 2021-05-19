namespace VolunteerComputing.Shared.Models
{
    public class DeviceData
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public bool IsWindows { get; set; } //otherwise Linux
        public bool CpuWorks { get; set; }
        public bool CpuAvailable { get; set; }

        public bool GpuWorks { get; set; }
        public bool GpuAvailable { get; set; }
        //energy efficient
        //speed efficient
    }
}
