using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VolunteerComputing.TaskServer
{
    public class Utils
    {
        static uint[] lookup32 = Enumerable.Range(0, 255)
           .Select(i =>
           {
               string s = i.ToString("X2");
               return s[0] + ((uint)s[1] << 16);
           })
           .ToArray();
        static SHA256 sha = SHA256.Create();

        static string ByteArrayToHex(byte[] bytes)
        {
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        public static string ComputeSha256(string input)
        {
            return ByteArrayToHex(sha.ComputeHash(Encoding.Unicode.GetBytes(input)));
        }
    }
}
