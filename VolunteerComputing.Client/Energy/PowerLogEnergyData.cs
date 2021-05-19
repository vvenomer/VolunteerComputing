using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VolunteerComputing.Client.Energy
{
    class PowerLogEnergyData : EnergyData
    {
        public TimeSpan Time { get; set; }
        public double Jules { get; set; } //Watt * Time.TotalSeconds
        public double Mwh { get; set; } //Watt * Time.TotalSeconds / 3600 * 1000
        public double Frequency { get; set; }

        public PowerLogEnergyData(string output, Dictionary<string, IEnumerable<string>> samples, Dictionary<string, double> otherData)
        {
            Time = TimeSpan.FromSeconds(otherData["Total Elapsed Time (sec)"]);
            Jules = otherData["Cumulative Processor Energy_0 (Joules)"];
            Mwh = otherData["Cumulative Processor Energy_0 (mWh)"];
            Watt = otherData["Average Processor Power_0 (Watt)"];
            Frequency = otherData["Measured RDTSC Frequency (GHz)"];
            Utilization = samples["CPU Utilization(%)"]
                .Select(s => double.Parse(s, NumberFormatInfo.InvariantInfo))
                .Average();
            PowerLimit = double.Parse(
                output
                    .Split("\n")
                    .FirstOrDefault(s => s.Contains("TDP"))
                    .Split(" = ")[1],
                 NumberFormatInfo.InvariantInfo);
        }
    }
}
