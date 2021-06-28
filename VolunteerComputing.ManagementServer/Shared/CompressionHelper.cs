using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace VolunteerComputing.Shared
{
    public class CompressionHelper
    {
        public static byte[] CompressFile(byte[] data)
        {
            var output = new MemoryStream();
            {
                using var stream = new GZipStream(output, CompressionLevel.Optimal);
                stream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        public static byte[] DecompressFile(byte[] data)
        {
            var output = new MemoryStream();
            {
                using var stream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
                stream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static byte[] Compress(string str)
        {
            var data = Encoding.Unicode.GetBytes(str);
            var output = new MemoryStream();
            {
                using var stream = new GZipStream(output, CompressionLevel.Optimal);
                stream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        public static string Decompress(byte[] data)
        {
            var output = new MemoryStream();
            {
                using var stream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
                stream.CopyTo(output);
            }

            return Encoding.Unicode.GetString(output.ToArray());
        }
    }
}
