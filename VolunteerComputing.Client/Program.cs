using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VolunteerComputing.Client.Energy;
using VolunteerComputing.Shared.Models;

namespace VolunteerComputing.Client
{
    class Program
    {
        static HashSet<(int, bool)> programs = new();
        static readonly string inputFile = "input.txt";
        static readonly string outputFile = "output.txt";
        static bool isIntel = true;
        static bool isCuda = true;
        static bool isWindows;
        static async Task Main(string[] args)
        {
            var os = IsWindowsOrLinux();
            if(!os.HasValue)
            {
                Console.WriteLine($"Error: unsupported OS - {RuntimeInformation.OSDescription}. Please use Windows or Linux");
                return;
            }
            isWindows = os.Value;

            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            isIntel = ParsePlatformOption(config, "intel");
            isCuda = ParsePlatformOption(config, "cuda");

            Console.WriteLine($"Starting on {(isWindows ? "Windows" : "Linux")} OS{(isIntel ? " with Intel CPU" : "")}{(isCuda ? " with Nvidia GPU" : "")}");
            /*var energyData = await EnergyMeasurer.RunPowerLog(
                @"D:\Intel\Power Gadget 3.6\PowerLog3.0.exe",
                TimeSpan.FromSeconds(3));

            var gpuEnergyData = await EnergyMeasurer.RunNvidiaSmi(
                @"C:\Program Files\NVIDIA Corporation\NVSMI\nvidia-smi.exe");*/

            var conn = new HubConnectionBuilder()
                .WithUrl("https://localhost:8080/tasks")
                .WithAutomaticReconnect()
                .Build();
            do
            {
                try
                {
                    await conn.StartAsync();
                    break;
                }
                catch
                {
                }
            } while (true);

            conn.On("SendTaskAsync", (int programId, string data, bool useCpu) => { Task.Run(async () => await CalculateTask(conn, programId, data, useCpu)); } );
            
            await conn.SendAsync("SendDeviceData", isWindows, isIntel, isCuda);

            while (true) await Task.Delay(500);
        }

        static bool? IsWindowsOrLinux()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return false;
            return null;
        }

        static bool ParsePlatformOption(IConfigurationRoot config, string option, bool defaultValue = true)
        {
            if (config.GetSection(option).Exists())
            {
                if (bool.TryParse(config[option], out var a))
                    return a;
                else
                {
                    Console.WriteLine($"Failed to parse value {config[option]} for option {option} - assuming {defaultValue}");
                }
            }
            return defaultValue;
        }

        static async Task CalculateTask(HubConnection connection, int programId, string data, bool useCpu)
        {
            try
            {
                var device = useCpu ? "CPU" : "GPU";
                Console.WriteLine($"Starting work on program id: {programId} using {device}");

                var dir = $"program{programId}{device}";
                Directory.CreateDirectory(dir);

                var file = Path.Combine(dir, "program.exe");
                var inputFilePath = Path.Combine(dir, inputFile);
                var outputFilePath = Path.Combine(dir, outputFile);

                if (!programs.Contains((programId, useCpu)))
                {
                    await DownloadProgram(connection, programId, useCpu, dir, file);
                    programs.Add((programId, useCpu));
                }

                File.WriteAllText(inputFilePath, data);

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = $"--inputFile {inputFile} --outputFile {outputFile}",
                    WorkingDirectory = dir
                    //RedirectStandardError = true,
                    //RedirectStandardOutput = true
                });
                //process.OutputDataReceived += (o, args) => Console.WriteLine(args.Data);
                //process.ErrorDataReceived += (o, args) => Console.WriteLine("Error: " + args.Data);
                await process.WaitForExitAsync();

                if (!File.Exists(outputFilePath))
                {
                    Console.WriteLine("Something went wrong"); //to do - handle
                                                               //send message, that failed
                    return;
                }
                var result = File.ReadAllText(outputFilePath);
                File.Delete(inputFilePath);
                File.Delete(outputFilePath);
                Console.WriteLine($"Finished work, sending results");
                await connection.SendAsync("SendResult", result, programId, useCpu);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static async Task DownloadProgram(HubConnection connection, int programId, bool useCpu, string dir, string file)
        {
            var programData = await connection.InvokeAsync<ProgramData>("GetProgram", programId, isWindows, useCpu);
            if (programData.ExeName is null)
            {
                File.WriteAllBytes(file, Convert.FromBase64String(programData.Program));
            }
            else
            {
                var zipFile = Path.GetRandomFileName() + ".zip";
                File.WriteAllBytes(zipFile, Convert.FromBase64String(programData.Program));
                var filesInDir = Directory.GetFiles(dir);
                foreach (var fileInDir in filesInDir)
                {
                    File.Delete(fileInDir);
                }
                ZipFile.ExtractToDirectory(zipFile, dir);
                File.Delete(zipFile);
                File.Move(Path.Combine(dir, programData.ExeName), file);
            }
        }
    }
}
