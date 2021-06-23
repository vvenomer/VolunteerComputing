using System;
using System.IO;
using System.Threading;

namespace VolunteerComputing.Awaiter
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileToAwait = args[0];
            while (!File.Exists(fileToAwait))
                Thread.Sleep(100);
        }
    }
}
