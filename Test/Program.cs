using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = Task.Run(() =>
            {
                Console.WriteLine("I'm just trying to write some data");
                File.WriteAllText("file.txt", "text");
                Console.WriteLine($"Yay, file {(File.Exists("file.txt") ? "exists" : "doesn't exist")}");
            });
            var otherTask = Task.Run(() =>
            {
                Console.WriteLine("While I write some data in other way");
                var file = File.Create("file2.txt");
                file.Write(Encoding.ASCII.GetBytes("text"));
                Console.WriteLine("Written");
                file.Flush();
                Console.WriteLine("Flushed");
                file.Close();
                Console.WriteLine("Closed");
                file.Dispose();
                Console.WriteLine($"Yay2, file {(File.Exists("file2.txt") ? "exists" : "doesn't exist")}");
            });
            otherTask.Wait();
            task.Wait();
        }
    }
}
