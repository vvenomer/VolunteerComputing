using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared.Models
{
    public class BundleResult : IEquatable<BundleResult>
    {
        public int Id { get; set; }
        public byte[] DataHash { get; private set; }

        public PacketBundle Bundle { get; set; }

        public ICollection<Packet> Packets { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BundleResult bundleResult &&
                   Equals(bundleResult);
        }

        public bool Equals(BundleResult other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public BundleResult SetDataHash(byte[] data)
        {
            using var sha = SHA256.Create();
            DataHash = sha.ComputeHash(data);
            return this;
        }
    }
}
