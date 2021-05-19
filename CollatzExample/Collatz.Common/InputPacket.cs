using System;

namespace Collatz.Shared
{
    public class InputPacket
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int NumberOfResultsInPacket { get; set; }
        public int NumberOfPackets { get; set; }

        public int NextCount() => Math.Min(End - Start, NumberOfPackets * NumberOfResultsInPacket);
        public int NextPackets(int count) => (count + NumberOfResultsInPacket - 1) / NumberOfResultsInPacket;
        public int NextResults(int packetId, int count, int packets) => packets - 1 != packetId ? NumberOfResultsInPacket : (count - (packets - 1) * NumberOfResultsInPacket); 
    }
}
