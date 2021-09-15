using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VolunteerComputing.Client.Energy;
using VolunteerComputing.Shared;
using VolunteerComputing.Shared.Dto;
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
        const int simulateErrorEvery = 100;
        static async Task Main(string[] args)
        {
            if (!LoadOperatingSystemInformation())
                return;

            LoadDeviceInfo(args);

            Console.WriteLine($"Starting on {(isWindows ? "Windows" : "Linux")} OS" +
                $"{(isIntel ? " with Intel CPU" : "")}{(isCuda ? " with Nvidia GPU" : "")}");

            if (isCuda && Storage.GpuEnergyToolPath == null)
            {
                Storage.GpuEnergyToolPath = FindNvidiaSmiPath(isWindows);
                Storage.HasSentInitMeasurements = false;
            }
            if (isIntel && Storage.CpuEnergyToolPath == null)
            {
                if(isWindows)
                    Storage.CpuEnergyToolPath = await FindPowerLogPath();
                else
                    Storage.CpuEnergyToolPath = FindPerfPath();
                Storage.HasSentInitMeasurements = false;
            }

            if ((isIntel && Storage.CpuEnergyToolPath == null) || (isCuda && Storage.GpuEnergyToolPath == null))
                return;

            double? cpuEnergy = -1, gpuEnergy = -1;
            if(!Storage.HasSentInitMeasurements)
            {
                Console.WriteLine("Running initial energy consumption test");

                var energyDataAwaitable = EnergyMeasurer.RunInitMeasurement(
                    Storage.GpuEnergyToolPath,
                    Storage.CpuEnergyToolPath,
                    isWindows,
                    initTestTime);

                var (cpuEnergyData, gpuEnergyData) = await energyDataAwaitable;
                (cpuEnergy, gpuEnergy) = (cpuEnergyData?.Watt, gpuEnergyData?.Watt);
                Console.WriteLine($"Cpu uses {cpuEnergy?.ToString("0.00") ?? "N/A"}/{cpuEnergyData?.PowerLimit.ToString() ?? "N/A"} W, " +
                    $"while Gpu uses {gpuEnergy?.ToString("0.00") ?? "N/A"}/{gpuEnergyData?.PowerLimit.ToString() ?? "N/A"} W");
            }

            var conn = await CreateHubConnection();
            Console.WriteLine("Connected to server");

            conn.On("SendTaskAsync", (int programId, byte[] data, bool useCpu) =>
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
                BaseCpuEnergyConsumption = cpuEnergy??-1,
                BaseGpuEnergyConsumption = gpuEnergy??-1
            };

            var id = await conn.InvokeAsync<int>("SendDeviceData", data);
            if(id == -1)
            {
                Console.WriteLine("Server doesn't know that device, please restart application to create new device");
                Storage.Restart();
                return;
            }
            if (cpuEnergy != -1 || gpuEnergy != -1)
                Storage.HasSentInitMeasurements = true;
            Storage.Id = id;
            while (true) await Task.Delay(500);
        }

        static string FindNvidiaSmiPath(bool isWindows)
        {
            var file = "nvidia-smi";
            if (isWindows)
            {
                file += ".exe";
                var path = @$"C:\Program Files\NVIDIA Corporation\NVSMI\{file}";

                if (File.Exists(path))
                    return path;

                var alternativePath = @"C:\Windows\System32\DriverStore\FileRepository";
                var driverStartsWith = @"nv";

                foreach (var driver in Directory.GetDirectories(alternativePath).Where(x => new DirectoryInfo(x).Name.StartsWith(driverStartsWith)))
                {
                    var pathInDriver = Path.Combine(alternativePath, driver, file);
                    if (File.Exists(pathInDriver))
                        return pathInDriver;
                }
            }
            else
            {
                if(CheckBin(file))
                    return file;
            }
            return AskForPath(file, "drivers for Nvidia graphic card");
        }

        static async Task<string> FindPowerLogPath() //windows only
        {
            var dir = @"C\Program Files\Intel\Power Gadget 3.6";
            var file = "PowerLog3.0.exe";
            var path = Path.Combine(dir, file);
            if (File.Exists(path))
                return path;

            var programsPath = @"C:\ProgramData\Microsoft\Windows\Start Menu";

            static async Task<string> findPowerLog(string dir)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    if(file.Contains("Power Gadget") && file.EndsWith(".lnk"))
                    {
                        var powershellCommand = $"(New-Object -ComObject WScript.Shell).CreateShortcut('{file}').WorkingDirectory";
                        var result = await new ProcessStartInfo { FileName = "powershell", Arguments = $"-c \"{powershellCommand}\"" }.RunProcess();
                        return result.Trim();
                    }
                }
                return Directory.GetDirectories(dir).Select(d => findPowerLog(d).Result).FirstOrDefault(p => p is not null);
            }

            dir = await findPowerLog(programsPath);
            if (dir is not null)
            {
                path = Path.Combine(dir, file);
                if (File.Exists(path))
                    return path;
            }

            return AskForPath(file, "Power Gadget installed");
            //return @"D:\Intel\Power Gadget 3.6\PowerLog3.0.exe";
        }

        static string FindPerfPath() //linux only
        {
            var file = "perf";
            if(CheckBin(file))
                return file;
            return AskForPath(file, "perf tool installed");
        }

        static bool CheckBin(string tool)
        {
            return File.Exists($"/usr/bin/{tool}") || File.Exists($"/bin/{tool}");
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
                .AddMessagePackProtocol()
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

        static async Task CalculateTask(HubConnection connection, int programId, byte[] compressedData, bool useCpu)
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

                File.WriteAllText(inputFilePath, CompressionHelper.Decompress(compressedData));
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
                {
                    if(isWindows)
                        energyData = await EnergyMeasurer.RunPowerLog(Storage.CpuEnergyToolPath, calculate);
                    else
                        energyData = await EnergyMeasurer.RunPerf(Storage.CpuEnergyToolPath, calculate);
                }
                else
                    energyData = await EnergyMeasurer.RunNvidiaSmi(Storage.GpuEnergyToolPath, calculate);

                var time = stopwatch.Elapsed;
                if (!File.Exists(outputFilePath))
                {
                    Console.WriteLine($"Something went wrong after {time} using {energyData.Watt:0.00} W energy on {device}");
                    await connection.SendAsync("CalculationsFailed", useCpu);
                    return;
                }
                
                var result = CompressionHelper.Compress(File.ReadAllText(outputFilePath));
                File.Delete(inputFilePath);
                File.Delete(outputFilePath);
                Console.WriteLine($"Finished work after {time} using {energyData.Watt:0.00} W energy on {device}, sending results");
                await connection.SendAsync("SendResult", result, programId, useCpu, time.TotalSeconds, energyData.Watt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await connection.SendAsync("CalculationsFailed", useCpu);
                return;
            }
        }

        static async Task DownloadProgram(HubConnection connection, int programId, bool useCpu, string dir, string file)
        {
            var programData = await connection.InvokeAsync<ProgramData>("GetProgram", programId, isWindows, useCpu);
            if (programData.ExeName is null)
            {
                File.WriteAllBytes(file, CompressionHelper.DecompressData(programData.Program));
            }
            else
            {
                var zipFile = Path.GetRandomFileName() + ".zip";
                File.WriteAllBytes(zipFile, CompressionHelper.DecompressData(programData.Program));
                var filesInDir = Directory.GetFiles(dir);
                foreach (var fileInDir in filesInDir)
                {
                    File.Delete(fileInDir);
                }
                ZipFile.ExtractToDirectory(zipFile, dir);
                File.Delete(zipFile);
                var exePath = Path.Combine(dir, programData.ExeName);
                if(!File.Exists(exePath))
                    exePath += ".exe";
                if(!File.Exists(exePath))
                    exePath = Path.Combine(dir, Path.GetFileNameWithoutExtension(programData.ExeName));
                
                File.Move(exePath, file);
            }
            if(!isWindows)
            {
                await Process.Start("/bin/bash", $"-c \"chmod 775 {file}\"").Await();
            }
        }
    }
}
