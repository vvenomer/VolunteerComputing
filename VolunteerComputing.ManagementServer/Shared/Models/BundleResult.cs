using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class BundleResult
    {
        public int Id { get; set; }
        public byte[] DataHash { get; private set; }

        public PacketBundle Bundle { get; set; }

        public ICollection<Packet> Packets { get; set; }

        public BundleResult SetDataHash(byte[] data)
        {
            using var sha = SHA256.Create();
            DataHash = sha.ComputeHash(data);
            return this;
        }
    }
}
