namespace VolunteerComputing.Shared.Models
{
    public class Packet
    {
        public long Id { get; set; }
        public PacketType Type { get; set; }
        public string Data { get; set; }
        public bool Aggregated { get; set; }
    }
}
