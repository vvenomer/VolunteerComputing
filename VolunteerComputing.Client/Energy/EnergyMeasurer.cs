using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VolunteerComputing.Client.Energy
{
    class EnergyMeasurer
    {
        public static async Task<NvidiaSmiEnergyData> RunNvidiaSmi(string path)
        {
            var output = await RunProcess(new ProcessStartInfo
            {
                FileName = path,
                Arguments = "-i 0 --query-gpu=\"utilization.gpu,utilization.memory,power.draw,power.limit\" --format=\"csv,noheader,nounits\""
            });
            var data = output
                .Split(",")
                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                .ToList();
            return new NvidiaSmiEnergyData(data[0], data[1], data[2], data[3]);
        }


        public static async Task<PowerLogEnergyData> RunPowerLog(string path, TimeSpan time)
        {
            var resultFile = Path.GetRandomFileName();
            var output = await RunProcess(new ProcessStartInfo
            {
                FileName = path,
                Arguments = $"-duration {time.TotalSeconds} -file {resultFile}",
            });
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

        static async Task<string> RunProcess(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            var process = Process.Start(processStartInfo);
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"{Path.GetFileNameWithoutExtension(processStartInfo.FileName)} exited with code: {process.ExitCode}");
            var output = await process.StandardOutput.ReadToEndAsync();
            return output;
        }
    }
}
