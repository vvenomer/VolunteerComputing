namespace VolunteerComputing.Shared.Models
{
    public class DeviceStat
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public double TimeSum { get; set; }
        public double EnergySum { get; set; }
        public bool IsCpu { get; set; } //or Gpu
        public int? ComputeTaskId { get; set; }
        public ComputeTask ComputeTask { get; set; }
        public DeviceData DeviceData { get; set; }

        public DeviceStat()
        {

        }

        public DeviceStat(DeviceData deviceData, ComputeTask computeTask, bool isCpu)
        {
            Count = 0;
            TimeSum = 0;
            EnergySum = 0;
            IsCpu = isCpu;
            DeviceData = deviceData;
            ComputeTask = computeTask;
        }

        public void AddStat(double time, double energy)
        {
            Count++;
            TimeSum += time;
            EnergySum += energy;
        }
    }
}
