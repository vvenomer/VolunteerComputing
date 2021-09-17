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

        static readonly Dictionary<ChoosingStrategy, Func<IEnumerable<DeviceWithStat>, double, int, DeviceWithStat>> StrategiesDictionary = new()
        {
            [ChoosingStrategy.Energy] = ChooseDeviceBasedOnEnergy,
            [ChoosingStrategy.Speed] = ChooseDeviceBasedOnSpeed,
            [ChoosingStrategy.Random] = ChooseDeviceRandom,
            [ChoosingStrategy.Energy60CutOff] = ChooseDeviceBasedOnEnergyWithCutOff60,
            [ChoosingStrategy.Energy80CutOff] = ChooseDeviceBasedOnEnergyWithCutOff80,
        };

        static readonly Dictionary<int, SortedSet<DeviceWithStat>> AllDevices = new();

        public static async Task RefreshDevices(IEnumerable<DeviceData> devices, VolunteerComputingContext context)
        {
            foreach (var device in devices)
            {
                await context.Entry(device).ReloadAsync();
                await context.Entry(device).Collection(x => x.DeviceStats).LoadAsync();
            }
        }

        public static DeviceWithStat ChooseDevice(IEnumerable<DeviceData> aviableDevices, ComputeTask computeTask)
        {
            var devices = GetDevices(aviableDevices, computeTask).ToList();

            //use some strategy
            var project = computeTask.Project;

            var strategy = StrategiesDictionary[project.ChoosingStrategy];

            return strategy(devices, project.ChanceToUseNewDevice, computeTask.Id);
        }

        static DeviceWithStat ChooseDeviceBasedOnEnergy(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice, int _)
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

        static DeviceWithStat ChooseDeviceBasedOnEnergyWithCutOff60(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice, int computeTaskId)
            => ChooseDeviceBasedOnEnergyWithCutOff(devices, chanceToUseNewDevice, computeTaskId, 0.6);
        static DeviceWithStat ChooseDeviceBasedOnEnergyWithCutOff80(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice, int computeTaskId)
            => ChooseDeviceBasedOnEnergyWithCutOff(devices, chanceToUseNewDevice, computeTaskId, 0.8);

        static DeviceWithStat ChooseDeviceBasedOnEnergyWithCutOff(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice, int computeTaskId, double cutOff)
        {
            if (!devices.Any())
                return null;
            if (!AllDevices.ContainsKey(computeTaskId))
                AllDevices[computeTaskId] = new SortedSet<DeviceWithStat>();
            var allDevices = AllDevices[computeTaskId];
            foreach (var device in devices)
            {
                if (allDevices.Contains(device))
                    allDevices.Remove(device);
                allDevices.Add(device);
            }

            Console.WriteLine(string.Join(",", devices.Select(x => x.Device.Id + (x.IsCpu ? "cpu" : "gpu") + ":" + x.EnergyEfficiency)));

            if (devices.Any(d => d.EnergyEfficiency == 0) && devices.Any(d => d.EnergyEfficiency > 0) && random.NextDouble() < chanceToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                return devices
                    .FirstOrDefault(d => d.EnergyEfficiency == 0);
            }
            else
            {
                var allDevicesCount = allDevices.Count;
                var exclude = allDevicesCount - (int)Math.Ceiling(allDevicesCount * cutOff);
                var toExclude = allDevices.TakeLast(exclude).ToList();

                Console.WriteLine($"Excluding {string.Join(", ", toExclude.Select(x => x.Device.Id + (x.IsCpu ? "cpu" : "gpu")))}, count {allDevicesCount}, exclude {exclude}");
                return devices
                    .OrderByDescending(d => d.EnergyEfficiency)
                    .FirstOrDefault(d => !toExclude.Contains(d));
            }
        }

        static DeviceWithStat ChooseDeviceRandom(IEnumerable<DeviceWithStat> devices, double _, int __)
        {
            var count = devices.Count();
            return count == 0 ? null : devices.ElementAt(random.Next(count));
        }

        static DeviceWithStat ChooseDeviceBasedOnSpeed(IEnumerable<DeviceWithStat> devices, double chanceToUseNewDevice, int _)
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
                            .FirstOrDefault(s => s.ComputeTaskId == computeTask.Id && p.isCpu == s.IsCpu))));
        }
    }
}
