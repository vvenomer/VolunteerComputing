using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VolunteerComputing.Client.Energy
{
    class EnergyMeasurer
    {
        public class RunEnergyData
        {
            public Process NvidiaSmiProcess { get; set; }
            public Process PowerLogProcess { get; set; }
            public string ResultFile { get; set; }
            public string AwaiterFile { get; set; }
        }

        public static async Task<(EnergyData, NvidiaSmiEnergyData)> RunInitMeasurement(string gpuToolPath, string cpuToolPath, bool isWindows, TimeSpan time)
        {
            var energyDataAwaitable = isWindows ? await RunPowerLog(cpuToolPath, time) : await RunPerf(cpuToolPath, time);
            var gpuEnergyDataAwaitable = RunNvidiaSmi(gpuToolPath, time);

            return (energyDataAwaitable, await gpuEnergyDataAwaitable);
        }

        public static async Task<NvidiaSmiEnergyData> RunNvidiaSmi(string path, Func<Task> action)
        {
            var process = NvidiaSmiStartInfo(path).Start();

            await action();

            process.Kill();
            var output = await process.Await().GetResultAsync();
            return ToNvidiaSmiEnergyData(output);
        }

        public static async Task<NvidiaSmiEnergyData> RunNvidiaSmi(string path, TimeSpan time)
        {
            var output = await NvidiaSmiStartInfo(path).RunProcessAndKill(time);
            return ToNvidiaSmiEnergyData(output);
        }

        public static async Task<EnergyData> RunPerf(string path, Func<Task> action)
        {
            var awaiterFile = Path.GetRandomFileName();
            var process = PerfStartInfo(path, $"./VolunteerComputing.Awaiter {awaiterFile}").Start();

            await action();
            File.Create(awaiterFile).Close();
            var output = await process.Await().GetResultFromErrorAsync();
            File.Delete(awaiterFile);
            return new PerfEnergyData(output);
        }

        public static async Task<EnergyData> RunPerf(string path, TimeSpan time)
        {
            var output = await PerfStartInfo(path, $"sleep {time.TotalSeconds}").RunProcessReadError();
            return new PerfEnergyData(output);        
        }

        public static async Task<PowerLogEnergyData> RunPowerLog(string path, Func<Task> action)
        {
            var resultFile = Path.GetRandomFileName();
            var awaiterFile = Path.GetRandomFileName();
            var process = new ProcessStartInfo
            {
                FileName = path,
                Arguments = $"-file {resultFile} -cmd VolunteerComputing.Awaiter {awaiterFile}",
            }.Start();

            await action();

            File.Create(awaiterFile).Close();
            var output = await process.Await().GetResultAsync();
            File.Delete(awaiterFile);
            return ToPowerLogEnergyData(output, resultFile);
        }

        public static async Task<PowerLogEnergyData> RunPowerLog(string path, TimeSpan time)
        {
            var resultFile = Path.GetRandomFileName();
            var output = await (new ProcessStartInfo
            {
                FileName = path,
                Arguments = $"-duration {time.TotalSeconds} -file {resultFile}",
            })
                .RunProcess();
            return ToPowerLogEnergyData(output, resultFile);
        }

        static ProcessStartInfo NvidiaSmiStartInfo(string path) => new()
        {
            FileName = path,
            Arguments = "-i 0 --query-gpu=\"utilization.gpu,utilization.memory,power.draw,power.limit\" --format=\"csv,noheader,nounits\" -lms 500"
        };

        static ProcessStartInfo PerfStartInfo(string path, string command) => new()
        {
            FileName = path,
            Arguments = $"stat -e power/energy-cores/,power/energy-pkg/,power/energy-ram/ -a {command}"
        }; //sudo turbostat -show CorWatt,PkgWatt,RAMWatt --quiet --enable CorWatt,PkgWatt,RAMWatt -S

        static PowerLogEnergyData ToPowerLogEnergyData(string output, string resultFile)
        {
            var csv = File.ReadAllLines(resultFile);
            File.Delete(resultFile);
            var endOfSamples = csv.ToList().IndexOf("");

            var samples = csv[0..endOfSamples]
                .Select(s => s.Split(","));
            var samplesHeaders = samples.First();
            var samplesData = samples.Skip(1);
            var samplesDict = samplesHeaders
                .Select((h, i) => (h, v: samplesData.Select(l => l[i])))
                .ToDictionary(kv => kv.h.Trim(), kv => kv.v);

            var other = csv[(endOfSamples + 1)..]
                .Where(s => s.Contains(" = "))
                .Select(s => s.Split(" = "))
                .ToDictionary(s => s[0], s => double.Parse(s[1], NumberFormatInfo.InvariantInfo));

            return new PowerLogEnergyData(output, samplesDict, other);
        }

        static NvidiaSmiEnergyData ToNvidiaSmiEnergyData(string output)
        {
            var data =
                output
                .Split("\n")
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .Select(x => x
                    .Split(",")
                    .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                    .ToList())
                .ToList();
            var averaged = Enumerable.Range(0, 4)
                .Select(i => data.Average(x => x[i]))
                .ToList();

            return new NvidiaSmiEnergyData(averaged[0], averaged[1], averaged[2], averaged[3]);
        }
    }
}
