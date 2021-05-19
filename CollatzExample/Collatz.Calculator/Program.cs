using Collatz.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolunteerComputing.ImplementationHelpers;

namespace Collatz.Calculator
{
    class Program
    {
        const int packetSize = 256 * 256;
        const int maxSteps = 10_000_000;
        static async Task Main(string[] args)
        {
            var inputParser = new InputParser<Numbers>(args);

            var config = Config.FromFile("config.json");
            var isCpu = config.IsCpu;

            var input = inputParser.FirstInput;
            List<int> results = null;

            /*GpuCalculator gpuCalculator = null;
            if (!isCpu)
                gpuCalculator = new GpuCalculator(packetSize, maxSteps);

            if (inputParser.IsIntel && inputParser.IsCuda)
            {
                results = new Dictionary<int, int>();
                Task<Dictionary<int, int>> cpuTask = null;
                for (int i = 0; i < input.Count; i += packetSize)
                {
                    var count = input.Count - i;
                    count = count > packetSize ? packetSize : count;

                    var canStartOnGpu = !gpuCalculator.IsWorking;
                    var canStartOnCpu = cpuTask?.IsCompleted ?? true;
                    if (!(canStartOnCpu || canStartOnGpu))
                    {
                        var id = Task.WaitAny(new[] { cpuTask, gpuCalculator.WaitUntilFree() });
                        if (id == 0)
                            canStartOnCpu = true;
                        else
                            canStartOnGpu = true;
                    }
                    var start = i + input.Start;
                    if (canStartOnGpu)
                    {
                        Console.WriteLine($"Using GPU start: {start} count: {count}");
                        gpuCalculator.Proces(start, count);
                    }
                    else
                    {
                        Console.WriteLine($"Using CPU start: {start} count: {count}");
                        if (cpuTask is not null)
                        {
                            results = results
                                .Union(cpuTask.Result)
                                .ToDictionary(x => x.Key, x => x.Value);
                        }
                        cpuTask = CpuProcessor(start, count);
                    }
                }
                if (cpuTask is not null)
                {
                    results = results
                        .Union(cpuTask.Result)
                        .ToDictionary(x => x.Key, x => x.Value);
                }
                await gpuCalculator.End();
                results = results
                    .Union(gpuCalculator.Results.ToDictionary(x => x.Item1, x => x.Item2))
                    .ToDictionary(x => x.Key, x => x.Value);
            }*/
            if (isCpu)
            {
                results = CpuProcessor(input.Start, input.Count);
            }
            else
            {
                results = (await GpuCalculator.CalculateOnce(input.Start, input.Count, packetSize, maxSteps));
            }

            if (results.Count != input.Count)
                Console.WriteLine($"Expected count {input.Count}, but got: {results.Count}");

            new ResponseBuilder()
                .AddSingle(new Result() { Numbers = new List<Numbers> { input }, Steps = new List<List<int>> { results } })
                .Save(inputParser.OutputFile);
        }

        static List<int> CpuProcessor(int start, int count)
        {
            return Enumerable.Range(0, count)
                    .AsParallel()
                    .AsOrdered()
                    .Select(i => start + i)
                    .Select(x => FasterCollatz(x, maxSteps))
                    .ToList();
        }

        static int Collatz(int input, int maxSteps)
        {
            for (int i = 0; i < maxSteps; i++)
            {
                if (input % 2 == 0)
                    input /= 2;
                else
                    input = 3 * input + 1;

                if (input == 1)
                    return i;
            }
            return -1;
        }

        static int FasterCollatz(int input, int maxSteps)
        {
            for (int i = 0; i < maxSteps; i++)
            {
                if ((input & 1) == 1)
                    input = (input << 1) + input + 1;
                else
                    input >>= 1;

                if (input == 1)
                    return i;
            }
            return -1;
        }
    }
}
