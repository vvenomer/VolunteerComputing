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

        public static async Task RefreshDevices(IEnumerable<DeviceData> devices, VolunteerComputingContext context)
        {
            await Task.WhenAll(devices.Select(async d => await context.Entry(d).ReloadAsync()));
        }

        public static DeviceWithStat ChooseDevice(IEnumerable<DeviceData> aviableDevices, ComputeTask computeTask, double changeToUseNewDevice)
        {
            var devices = GetDevices(aviableDevices, computeTask).ToList();

            if (devices.Any(d => d.EnergyEfficiency == 0) && devices.Any(d => d.EnergyEfficiency > 0) && random.NextDouble() < changeToUseNewDevice)
            {
                //there are some devices already checked and some new ones - try new one
                return devices
                    .FirstOrDefault(d => d.EnergyEfficiency == 0);
            }
            else
            {
                //special case for when there are no devices?
                return devices
                    .OrderByDescending(d => d.EnergyEfficiency)
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
