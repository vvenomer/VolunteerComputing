using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VolunteerComputing.ManagementServer.Server
{
    public class ResultsHelper
    {
        static string resultsPath = "Results";

        public static async Task<string> SaveResult(string projectName, string result)
        {
            Directory.CreateDirectory(resultsPath);
            var fileId = Path.GetRandomFileName();
            await File.WriteAllTextAsync(Path.Combine(resultsPath, $"{fileId}.{projectName}_result.json"), result);
            return fileId;
        }

        public static byte[] ReadResult(string fileId, string projectName)
        {
            return File.ReadAllBytes(Path.Combine(resultsPath, $"{fileId}.{projectName}_result.json"));
        }
    }
}
