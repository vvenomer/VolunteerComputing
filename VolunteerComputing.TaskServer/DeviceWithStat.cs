using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.TaskServer
{
    public class DeviceWithStat
    {
        double time;
        double energy;
        public DeviceWithStat(DeviceData deviceData, bool isCpu, DeviceStat stat)
        {
            Device = deviceData;
            IsCpu = isCpu;
            if (stat is null || stat.Count == 0)
            {
                time = energy = double.PositiveInfinity;
            }
            else
            {
                var baseConsumption = (stat.IsCpu ? Device.BaseCpuEnergyConsumption : Device.BaseGpuEnergyConsumption);

                time = stat.TimeSum / stat.Count;
                energy = stat.EnergySum / stat.Count - baseConsumption;
            }
            SpeedEfficiency = 1 / time;
            EnergyEfficiency = 1 / time / energy;
            EnergyWithoutTime = 1 / time;
        }

        public DeviceData Device { get; }
        public bool IsCpu { get; }
        public double SpeedEfficiency { get; }
        public double EnergyEfficiency { get; }
        public double EnergyWithoutTime { get; }
    }
}
