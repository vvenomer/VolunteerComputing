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

        public static async Task<Process> Await(this Task<Process> processTask)
        {
            var process = await processTask;
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
            await process.CheckForError();
            var output = await process.StandardOutput.ReadToEndAsync();
            return output;
        }

        public static async Task<string> GetResultFromErrorAsync(this Task<Process> processTask)
        {
            return await (await processTask).GetResultFromErrorAsync();
        }

        public static async Task<string> GetResultFromErrorAsync(this Process process)
        {
            await process.CheckForError();
            var output = await process.StandardError.ReadToEndAsync();
            return output;
        }

        private static async Task CheckForError(this Process process)
        {
            //137 - exit code for nvidia-smi for linux
            //-1 - exit code for nvidia-smi for windows
            if (process.ExitCode != 137 && process.ExitCode != -1 && process.ExitCode != 0)
            {
                var file = Path.GetFileNameWithoutExtension(process.StartInfo.FileName);
                var error = await process.StandardError.ReadToEndAsync();
                var output = await process.StandardOutput.ReadToEndAsync();
                var message = $"{file} exited with code: {process.ExitCode}";
                if(!string.IsNullOrWhiteSpace(output))
                    message += ", output: " + output;
                if(!string.IsNullOrWhiteSpace(error))
                    message += ", error: " + error;
                throw new Exception(message);
            }
        }

        public static async Task<string> RunProcess(this ProcessStartInfo processStartInfo)
        {
            return await processStartInfo
                .Start()
                .Await()
                .GetResultAsync();
        }

        public static async Task<string> RunProcessReadError(this ProcessStartInfo processStartInfo)
        {
            return await processStartInfo
                .Start()
                .Await()
                .GetResultFromErrorAsync();
        }

        public static async Task<string> RunProcessAndKill(this ProcessStartInfo processStartInfo, TimeSpan killTime)
        {
            return await processStartInfo
                .Start()
                .WaitThenKill(killTime)
                .Await()
                .GetResultAsync();
        }
    }
}
