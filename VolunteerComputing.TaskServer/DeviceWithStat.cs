using System;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.TaskServer
{
    public class DeviceWithStat : IComparable<DeviceWithStat>, IEquatable<DeviceWithStat>
    {
        public DeviceWithStat(DeviceData deviceData, bool isCpu, DeviceStat stat)
        {
            Device = deviceData;
            IsCpu = isCpu;
            double time;
            double energy;
            if (stat is null || stat.Count == 0)
            {
                time = energy = double.PositiveInfinity;
            }
            else
            {
                var baseConsumption = (stat.IsCpu ? Device.BaseCpuEnergyConsumption : Device.BaseGpuEnergyConsumption);

                time = stat.TimeSum / stat.Count;
                energy = stat.EnergySum / stat.Count - baseConsumption;
                if(energy < 0)
                    energy = double.Epsilon;
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

        public int CompareTo(DeviceWithStat other)
        {
            return EnergyEfficiency.CompareTo(other.EnergyEfficiency);
        }

        public bool Equals(DeviceWithStat other)
        {
            return Device.Id == other.Device.Id
                && IsCpu == other.IsCpu;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceWithStat);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Device.Id, IsCpu);
        }
    }
}
