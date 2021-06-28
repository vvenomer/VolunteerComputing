using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Shared
{
    public class ShareAPI
    {
        static readonly string uploadDirectory = @"C:\Users\Pawelb\Desktop\Programowanie\csharp\VolunteerComputing\Upload";
        public static string GetFromShare(string path)
        {
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }
        public static string GetTextFromShare(string path)
        {
            return File.ReadAllText(path);
        }

        public static string SaveToShare(string file)
        {
            var program = Convert.FromBase64String(file);
            var path = Path.Combine(uploadDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(path, program);
            return path;
        }
        public static string SaveTextToShare(string text)
        {
            var path = Path.Combine(uploadDirectory, Path.GetRandomFileName());
            File.WriteAllText(path, text);
            return path;
        }
    }
}
