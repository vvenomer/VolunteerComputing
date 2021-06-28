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
        static TimeSpan initTestTime = TimeSpan.FromSeconds(5);
        static async Task Main(string[] args)
        {
            if (!LoadOperatingSystemInformation())
                return;

            LoadDeviceInfo(args);

            Console.WriteLine($"Starting on {(isWindows ? "Windows" : "Linux")} OS" +
                $"{(isIntel ? " with Intel CPU" : "")}{(isCuda ? " with Nvidia GPU" : "")}");

            if (Storage.GpuEnergyToolPath == null)
                Storage.GpuEnergyToolPath = FindNvidiaSmiPath(isWindows);
            if (Storage.CpuEnergyToolPath == null)
                Storage.CpuEnergyToolPath = FindPowerLogPath();

            if (Storage.CpuEnergyToolPath == null || Storage.GpuEnergyToolPath == null)
                return; //todo: check only if aviable

            double cpuEnergy = 0, gpuEnergy = 0;
            if(!Storage.HasSentInitMeasurements)
            {
                Console.WriteLine("Running initial energy consumption test");

                var energyDataAwaitable = EnergyMeasurer.RunInitMeasurement(
                    Storage.GpuEnergyToolPath,
                    Storage.CpuEnergyToolPath,
                    initTestTime);

                var (cpuEnergyData, gpuEnergyData) = await energyDataAwaitable;
                (cpuEnergy, gpuEnergy) = (cpuEnergyData.Watt, gpuEnergyData.Watt);
                Console.WriteLine($"Cpu uses {cpuEnergy:0.00}/{cpuEnergyData.PowerLimit} W, " +
                    $"while Gpu uses {gpuEnergy:0.00}/{gpuEnergyData.PowerLimit} W");
                Storage.HasSentInitMeasurements = true;
            }

            var conn = await CreateHubConnection();

            conn.On("SendTaskAsync", (int programId, string data, bool useCpu) =>
            {
                Task.Run(async () => await CalculateTask(conn, programId, data, useCpu));
            });

            conn.On("InformFinished", () => 
                Console.WriteLine("There is no more work to be done. You can wait for more or check back in later."));

            var data = new DeviceData
            {
                Id = Storage.Id,
                IsWindows = isWindows,
                CpuAvailable = isIntel,
                GpuAvailable = isCuda,
                BaseCpuEnergyConsumption = cpuEnergy,
                BaseGpuEnergyConsumption = gpuEnergy
            };

            var id = await conn.InvokeAsync<int>("SendDeviceData", data);
            Storage.Id = id;
            while (true) await Task.Delay(500);
        }

        static string FindNvidiaSmiPath(bool isWindows)
        {
            if (isWindows)
            {
                var path = @"C:\Program Files\NVIDIA Corporation\NVSMI\nvidia-smi.exe";

                if (File.Exists(path))
                    return path;

                var alternativePath = @"C:\Windows\System32\DriverStore\FileRepository";
                var driverStartsWith = @"nvdm";
                var file = "nvidia-smi.exe";

                foreach (var driver in Directory.GetDirectories(alternativePath).Where(x => x.StartsWith(driverStartsWith)))
                {
                    var pathInDriver = Path.Combine(alternativePath, driver, file);
                    if (File.Exists(pathInDriver))
                        return pathInDriver;
                }
                return AskForPath(file, "drivers for Nvidia graphic card");
            }
            else
                return "nvidia-smi";
        }

        static string FindPowerLogPath() //windows only
        {
            var dir = @"C\Program Files\Intel\Power Gadget 3.6";
            var file = "PowerLog3.0.exe";
            var path = Path.Combine(dir, file);
            if (File.Exists(path))
                return path;
            return AskForPath(file, "Power Gadget installed");
            //return @"D:\Intel\Power Gadget 3.6\PowerLog3.0.exe";
        }

        private static string AskForPath(string file, string requirement)
        {
            Console.WriteLine($"Couldn't find {file}. Please make sure you have {requirement}.");
            Console.Write($"Do you want to give custom path to {file}? [y/n] ");
            var result = Console.ReadLine();
            if (result.ToLower().Trim() == "y")
            {
                while (true)
                {
                    Console.Write("Custom path: ");
                    var customPath = Console.ReadLine();
                    if (File.Exists(customPath))
                        return customPath;
                    if (File.Exists(Path.Combine(customPath, file)))
                        return Path.Combine(customPath, file);
                    Console.WriteLine("Path doesn't exist");
                }
            }
            return null;
        }

        static async Task<HubConnection> CreateHubConnection()
        {
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
            return conn;
        }

        static void LoadDeviceInfo(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            isIntel = ParsePlatformOption(config, "intel");
            isCuda = ParsePlatformOption(config, "cuda");
        }

        static bool LoadOperatingSystemInformation()
        {
            var os = IsWindowsOrLinux();
            if (!os.HasValue)
            {
                Console.WriteLine($"Error: unsupported OS - {RuntimeInformation.OSDescription}. Please use Windows or Linux");
                return false;
            }
            isWindows = os.Value;
            return true;
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
                var stopwatch = new Stopwatch();

                async Task calculate()
                {
                    stopwatch.Start();

                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = file,
                        Arguments = $"--inputFile {inputFile} --outputFile {outputFile}",
                        WorkingDirectory = dir,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    });
                    //process.OutputDataReceived += (o, args) => Console.WriteLine(args.Data);
                    //process.ErrorDataReceived += (o, args) => Console.WriteLine("Error: " + args.Data);
                    await process.WaitForExitAsync();
                    stopwatch.Stop();
                }

                EnergyData energyData;
                if (useCpu)
                    energyData = await EnergyMeasurer.RunPowerLog(Storage.CpuEnergyToolPath, calculate);
                else
                    energyData = await EnergyMeasurer.RunNvidiaSmi(Storage.GpuEnergyToolPath, calculate);

                var time = stopwatch.Elapsed;
                if (!File.Exists(outputFilePath))
                {
                    Console.WriteLine($"Something went wrong after {time} using {energyData.Watt} W energy on {device}");
                    //to do - handle
                    //send message, that failed
                    return;
                }
                var result = File.ReadAllText(outputFilePath);
                File.Delete(inputFilePath);
                File.Delete(outputFilePath);
                Console.WriteLine($"Finished work after {time} using {energyData.Watt} W energy on {device}, sending results");
                await connection.SendAsync("SendResult", result, programId, useCpu, time.TotalSeconds, energyData.Watt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        static async Task DownloadProgram(HubConnection connection, int programId, bool useCpu, string dir, string file)
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
