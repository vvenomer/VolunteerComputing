using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.Shared.Models;
using VolunteerComputing.TaskServer.Data;

namespace VolunteerComputing.TaskServer
{
    public class DeviceManager
    {
        static readonly Random random = new();

        static readonly Dictionary<ChoosingStrategy, Func<IEnumerable<DeviceWithStat>, double, DeviceWithStat>> StrategiesDictionary = new()
        {
            [ChoosingStrategy.Energy] = ChooseDeviceBasedOnEnergy,
            [ChoosingStrategy.Speed] = ChooseDeviceBasedOnSpeed,
            [ChoosingStrategy.Random] = ChooseDeviceRandom,
            [ChoosingStrategy.Energy60CutOff] = ChooseDeviceBasedOnEnergyWithCutOff60,
            [ChoosingStrategy.Energy80CutOff] = ChooseDeviceBasedOnEnergyWithCutOff80,
        };

        static readonly SortedSet<DeviceWithStat> AllDevices = new();

        public static async Task RefreshDevices(IEnumerable<DeviceData> devices, VolunteerComputingContext context)
        {
            foreach (var device in devices)
            {
                await context.Entry(device).ReloadAsync();
            }
        }

        public static DeviceWithStat ChooseDevice(IEnumerable<DeviceData> aviableDevices, ComputeTask computeTask)
        {
            var devices = GetDevices(aviableDevices, computeTask).ToList();

            //use some strategy
            var project = computeTask.Project;

            var strategy = StrategiesDictionary[project.ChoosingStrategy];

            return strategy(devices, project.ChanceToUseNewDevice);
        }

        static DeviceWithStat ChooseDeviceBasedOnEnergy(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice)
        {
            if (devices.Any(d => d.EnergyEfficiency == 0) && devices.Any(d => d.EnergyEfficiency > 0) && random.NextDouble() < chanceToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                return devices
                    .FirstOrDefault(d => d.EnergyEfficiency == 0);
            }
            else
            {
                return devices
                    .OrderByDescending(d => d.EnergyEfficiency)
                    .FirstOrDefault();
            }
        }

        static DeviceWithStat ChooseDeviceBasedOnEnergyWithCutOff60(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice)
            => ChooseDeviceBasedOnEnergyWithCutOff(devices, chanceToUseNewDevice, 0.6);
        static DeviceWithStat ChooseDeviceBasedOnEnergyWithCutOff80(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice)
            => ChooseDeviceBasedOnEnergyWithCutOff(devices, chanceToUseNewDevice, 0.8);

        static DeviceWithStat ChooseDeviceBasedOnEnergyWithCutOff(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice, double cutOff)
        {
            foreach (var device in devices.Where(d => d.EnergyEfficiency > 0))
            {
                if(AllDevices.Contains(device))
                    AllDevices.Remove(device);
                AllDevices.Add(device);
            }

            if (devices.Any(d => d.EnergyEfficiency == 0) && devices.Any(d => d.EnergyEfficiency > 0) && random.NextDouble() < chanceToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                return devices
                    .FirstOrDefault(d => d.EnergyEfficiency == 0);
            }
            else
            {
                var allDevicesCount = AllDevices.Count;
                var exclude = allDevicesCount - (int)Math.Ceiling(allDevicesCount * cutOff);
                var toExclude = AllDevices.TakeLast(exclude).ToList();
                return devices
                    .OrderByDescending(d => d.EnergyEfficiency)
                    .FirstOrDefault(d => !toExclude.Contains(d));
            }
        }

        static DeviceWithStat ChooseDeviceRandom(IEnumerable<DeviceWithStat> devices, double _)
        {
            var count = devices.Count();
            return count == 0 ? null : devices.ElementAt(random.Next(count));
        }

        static DeviceWithStat ChooseDeviceBasedOnSpeed(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice)
        {
            if (devices.Any(d => d.SpeedEfficiency == 0) && devices.Any(d => d.SpeedEfficiency > 0) && random.NextDouble() < chanceToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                return devices
                    .FirstOrDefault(d => d.SpeedEfficiency == 0);
            }
            else
            {
                return devices
                    .OrderByDescending(d => d.SpeedEfficiency)
                    .FirstOrDefault();
            }
        }

        static IEnumerable<DeviceWithStat> GetDevices(IEnumerable<DeviceData> aviableDevices, ComputeTask computeTask)
        {
            return computeTask
                .GetAvailablePlatforms()
                .SelectMany(p => aviableDevices
                    .Where(d => p.isWindows == d.IsWindows)
                    .Where(d => p.isCpu ? d.CpuAvailable : d.GpuAvailable)
                    .Where(d => (p.isCpu ? d.CpuWorksOnBundle : d.GpuWorksOnBundle) == 0)
                    .Select(d => new DeviceWithStat(
                        d,
                        p.isCpu,
                        d.DeviceStats
                            .FirstOrDefault(s => s.ComputeTask == computeTask && p.isCpu == s.IsCpu))));
        }
    }
}
