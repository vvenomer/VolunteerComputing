namespace VolunteerComputing.Shared.Models
{
    public class Packet
    {
        public long Id { get; set; }
        public int? TypeId { get; set; }
        public PacketType Type { get; set; }
        public string Data { get; set; }
        public bool Aggregated { get; set; }
        public DeviceData DeviceWorkedOnIt { get; set; } //so that it's not calculated by same users and so that you can group them
        public int? BundleId { get; set; }
        public PacketBundle Bundle { get; set; }
        public int? BundleResultId { get; set; }
        public BundleResult BundleResult { get; set; }
    }
}
