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
        static readonly string uploadDirectory = @"/home/vvenomer/Desktop/Programowanie/VolunteerComputing/Upload";
        public static byte[] GetFromShare(string path)
        {
            EnsurePathCreated();
            return File.ReadAllBytes(path);
        }
        public static string GetTextFromShare(string path)
        {
            EnsurePathCreated();
            return File.ReadAllText(path);
        }

        public static string SaveToShare(string file)
        {
            var program = Convert.FromBase64String(file);
            var path = Path.Combine(uploadDirectory, Path.GetRandomFileName());
            EnsurePathCreated();
            File.WriteAllBytes(path, program);
            return path;
        }
        public static string SaveTextToShare(string text)
        {
            var path = Path.Combine(uploadDirectory, Path.GetRandomFileName());
            EnsurePathCreated();
            File.WriteAllText(path, text);
            return path;
        }

        public static void RemoveFromShare(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (System.Exception)
            {

            }
        }

        private static void EnsurePathCreated()
        {
            Directory.CreateDirectory(uploadDirectory);
        }
    }
}
