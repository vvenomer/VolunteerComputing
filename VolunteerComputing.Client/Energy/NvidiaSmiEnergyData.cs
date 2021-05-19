namespace VolunteerComputing.Client.Energy
{
    class NvidiaSmiEnergyData : EnergyData
    {
        public double MemoryUtilization { get; set; }

        public NvidiaSmiEnergyData(double gpuUtil, double memUtil, double watt, double limit)
        {
            Utilization = gpuUtil;
            MemoryUtilization = memUtil;
            Watt = watt;
            PowerLimit = limit;
        }
    }
}
