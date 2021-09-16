using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VolunteerComputing.Shared.Models;

namespace StatsCalculator
{
    class Program
    {
        static Dictionary<int, string> deviceIdToName = new()
        {
            [15] = "des04",
            [16] = "des03",
            [17] = "des02",
            [18] = "my",
            [19] = "work",
            [20] = "apl09",
            [21] = "apl10",
            [22] = "des01"
        };

        static async Task Main(string[] args)
        {
            var http = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (h, c, ch, e) => true })
            {
                BaseAddress = new Uri("https://localhost:5001"),
                
            };
            var devices = await http.GetFromJsonAsync<IList<DeviceData>>("api/Devices");
            var projects = await http.GetFromJsonAsync<IList<Project>>("api/Projects");
            var results = await http.GetFromJsonAsync<IList<Result>>($"api/Results/ByProject/{projects[0].Id}");

            var projectLastTime = results.Last().SecondsElapsed;

            var procesingUnits = devices
                .SelectMany(d => d.DeviceStats.Select(s => new DeviceWithStat { DeviceData = d, DeviceStat = s }))
                .ToList();



            var energyUsedDuringFreeTimes = procesingUnits.Select(p => p.FullEstimatedEnergy(projectLastTime)).ToList();
            var energyUsedDuringFreeTime = energyUsedDuringFreeTimes.Sum();
        }

        class DeviceWithStat
        {
            public DeviceData DeviceData { get; set; }
            public DeviceStat DeviceStat { get; set; }

            public string PUid => deviceIdToName[DeviceData.Id] + "-" + (DeviceStat.IsCpu ? "cpu" : "gpu");

            public double AverageTime => DeviceStat.TimeSum / DeviceStat.Count;
            
            public double BaseEnergyConsumption => DeviceStat.IsCpu ? DeviceData.BaseCpuEnergyConsumption : DeviceData.BaseGpuEnergyConsumption;

            public double AveragePowerTotal => DeviceStat.EnergySum / DeviceStat.Count;
            public double AveragePower => DeviceStat.EnergySum / DeviceStat.Count - BaseEnergyConsumption;

            public double EstimatedEnergyWhileWorking => AveragePower * DeviceStat.TimeSum; //jules

            public double EstimatedEnergyWhileNotWorking(double fullTimeInSeconds) => BaseEnergyConsumption * (fullTimeInSeconds - DeviceStat.TimeSum);

            public double FullEstimatedEnergy(double fullTimeInSeconds) => EstimatedEnergyWhileWorking + EstimatedEnergyWhileNotWorking(fullTimeInSeconds);

            public double FullEstimatedEnergy2(double fullTimeInSeconds) => AveragePowerTotal * DeviceStat.TimeSum + EstimatedEnergyWhileNotWorking(fullTimeInSeconds - DeviceStat.TimeSum);
        }
    }
}
