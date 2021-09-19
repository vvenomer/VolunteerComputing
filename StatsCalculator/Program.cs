using System;
using System.Collections.Generic;
using System.IO;
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
            [34] = "des01",
            [35] = "des02",
            [36] = "des07",
            [37] = "des03",
            [38] = "apl09",
            [39] = "apl10",
            [40] = "my",
            [41] = "work",
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

            File.WriteAllText("index.html", ToHtmlTable(procesingUnits, projectLastTime));
        }

        static string ToHtmlTable(List<DeviceWithStat> procesingUnits, double fullTimeInSeconds)
        {
            var str = "<html><head><script src=\"https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js\"></script></head><body>" +
                "<table class=\"table table-striped\"><tr><th>Name</th><th>Time</th><th>Power</th><th>Count</th><th>Base</th><th>Avg Time</th><th>Avg Power Total</th><th>Avg Power</th><th>Estimated Energy While Working</th><th>Estimated Energy While Not Working</th><th>Full Estimated Energy</th></tr>";

            var estimatedEnergyWhileWorking = 0.0;
            var estimatedEnergyWhileNotWorking = 0.0;

            var energyUsedDuringFreeTime = 0.0;
            foreach (var procesingUnit in procesingUnits)
            {
                str += "<tr>";

                str += WrapTd(procesingUnit.PUid);
                str += WrapTd(procesingUnit.DeviceStat.TimeSum);
                str += WrapTd(procesingUnit.DeviceStat.EnergySum);
                str += WrapTd(procesingUnit.DeviceStat.Count);
                str += WrapTd(procesingUnit.BaseEnergyConsumption);
                str += WrapTd(procesingUnit.AverageTime);
                str += WrapTd(procesingUnit.AveragePowerTotal);
                str += WrapTd(procesingUnit.AveragePower);
                var x = procesingUnit.EstimatedEnergyWhileWorking;
                estimatedEnergyWhileWorking += x;
                str += WrapTd(x);
                x = procesingUnit.EstimatedEnergyWhileNotWorking(fullTimeInSeconds);
                estimatedEnergyWhileNotWorking += x;
                str += WrapTd(x);
                x = procesingUnit.FullEstimatedEnergy(fullTimeInSeconds);
                energyUsedDuringFreeTime += x;
                str += WrapTd(x);

                str += "</tr>";
            }
            return str + $"</table><br>Time Elapsed: {fullTimeInSeconds} s ({TimeSpan.FromSeconds(fullTimeInSeconds)}), Total Energy: {energyUsedDuringFreeTime} J, Total Working Energy: {estimatedEnergyWhileWorking} J, Total Not Working Energy: {estimatedEnergyWhileNotWorking} J, Average Power: {energyUsedDuringFreeTime / fullTimeInSeconds} W</body>";

        }

        static string WrapTd<T>(T str)
        {
            return $"<td>{str}</td>";
        }

        class DeviceWithStat
        {
            public DeviceData DeviceData { get; set; }
            public DeviceStat DeviceStat { get; set; }

            public string PUid => deviceIdToName[DeviceData.Id] + "-" + (DeviceStat.IsCpu ? "cpu" : "gpu") + "-" + DeviceStat.ComputeTaskId;

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
