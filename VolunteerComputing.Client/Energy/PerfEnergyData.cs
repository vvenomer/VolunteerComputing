using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace VolunteerComputing.Client.Energy
{
    class PerfEnergyData : EnergyData
    {

        public double Jules { get; set; } //Watt * Time.TotalSeconds
        public TimeSpan Time { get; set; }

        public PerfEnergyData(string output)
        {
            var splited = output.Split("\n");
            Jules = GetDoubleFromLines(splited, "energy-cores");
            Time = TimeSpan.FromSeconds(GetDoubleFromLines(splited, "time elapsed"));
            Watt = Jules / Time.TotalSeconds;
            Console.WriteLine("Jules: " + Jules + " Time: " + Time.TotalSeconds + " Watt: " + Watt);
            try
            {
                var microWatts = long.Parse(File.ReadAllText("/sys/class/powercap/intel-rapl/intel-rapl:0/constraint_0_max_power_uw"));
                PowerLimit = microWatts / 1_000_000.0;
            }
            catch (Exception) //might not be permitted
            {
                PowerLimit = double.NaN;
            }
        }

        static double GetDoubleFromLines(IEnumerable<string> lines, string selector)
        {
            var line = lines.FirstOrDefault(s => s.Contains(selector)).Trim();
            var number = line.Substring(0, line.IndexOf(" "));
            return double.Parse(number.Replace(',', '.'));
        }
    }
}
