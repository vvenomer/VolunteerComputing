using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Client.Energy
{
    static class ProcessHelper
    {
        public static Process Start(this ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            return Process.Start(processStartInfo);
        }

        public static async Task<Process> Await(this Process process)
        {
            await process.WaitForExitAsync();
            return process;
        }

        public static async Task<Process> WaitThenKill(this Process process, TimeSpan killTime)
        {
            await Task.Delay(killTime);
            process.Kill();
            return process;
        }

        public static async Task<string> GetResultAsync(this Task<Process> processTask)
        {
            return await (await processTask).GetResultAsync();
        }

        public static async Task<string> GetResultAsync(this Process process)
        {
            if (process.ExitCode != -1 && process.ExitCode != 0)
                throw new Exception($"{Path.GetFileNameWithoutExtension(process.StartInfo.FileName)} exited with code: {process.ExitCode}");
            var output = await process.StandardOutput.ReadToEndAsync();
            return output;
        }

        public static async Task<string> RunProcess(ProcessStartInfo processStartInfo)
        {
            return await processStartInfo
                .Start()
                .Await()
                .GetResultAsync();
            /*processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            var process = Process.Start(processStartInfo);

            if (process.ExitCode != 0)
                throw new Exception($"{Path.GetFileNameWithoutExtension(processStartInfo.FileName)} exited with code: {process.ExitCode}");
            var output = await process.StandardOutput.ReadToEndAsync();
            return output;*/
        }

        public static async Task<string> RunProcessAndKill(ProcessStartInfo processStartInfo, TimeSpan killTime)
        {
            return await processStartInfo
                .Start()
                .WaitThenKill(killTime)
                .GetResultAsync();
            /*processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            var process = Process.Start(processStartInfo);
            await Task.Delay(killTime);
            process.Kill();
            //await process.WaitForExitAsync();

            if (process.ExitCode != -1 && process.ExitCode != 0)
                throw new Exception($"{Path.GetFileNameWithoutExtension(processStartInfo.FileName)} exited with code: {process.ExitCode}");
            var output = await process.StandardOutput.ReadToEndAsync();
            return output;*/
        }
    }
}
