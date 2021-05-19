using Newtonsoft.Json;
using System.IO;

namespace Collatz.Shared
{
    public class Config
    {
        public static Config FromFile(string path)
        {
            if(File.Exists(path))
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            return null;
        }

        public bool IsCpu { get; set; }
    }
}
