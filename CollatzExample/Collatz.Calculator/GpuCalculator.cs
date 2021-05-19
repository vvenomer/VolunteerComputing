using Cuda;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Collatz.Calculator
{
    class GpuCalculator
    {
        public bool IsWorking { get; private set; } = true;
        Task task;
        BlockingCollection<(int, int)> collection = new();
        public List<(int, int)> Results { get; private set; } = new();

        public GpuCalculator(int maxCount, int maxSteps)
        {
            task = Task.Run(async () =>
            {
                var gpu = new Gpu();
                gpu.LoadModule("collatz.cu");
                var deviceSteps = gpu.AllocMemory<int>(maxCount);
                IsWorking = false;
                while (!collection.IsCompleted)
                {
                    if (collection.TryTake(out var a))
                    {
                        IsWorking = true;
                        var (start, count) = a;

                        while (true)
                        {
                            try
                            {
                                await gpu.RunFunctionAsync(
                                    "collatz",
                                    count,
                                    new object[] { start, count, deviceSteps.pointer, maxSteps });
                                Results.AddRange(deviceSteps.Get().Take(count).Select((s, i) => (i + start, s)));
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }

                    }
                    else
                        IsWorking = false;
                }
            });
        }

        public async Task WaitUntilFree()
        {
            await Task.Run(() => SpinWait.SpinUntil(() => !IsWorking));
        }

        public void Proces(int start, int count)
        {
            collection.Add((start, count));
        }

        public async Task End()
        {
            collection.CompleteAdding();
            await task;
        }

        public static async Task<List<int>> CalculateOnce(int start, int count, int maxSize, int maxSteps)
        {
            while (true)
            {
                try
                {
                    var gpu = new Gpu();
                    gpu.LoadModule("collatz.cu");
                    var deviceSteps = gpu.AllocMemory<int>(maxSize);
                    var results = new List<int>();
                    for (int i = 0; i < count; i += maxSize)
                    {
                        var c = count - i;
                        var currentCount = c > maxSize ? maxSize : c;
                        await gpu.RunFunctionAsync(
                            "collatz",
                            currentCount,
                            new object[] { start + i, currentCount, deviceSteps.pointer, maxSteps });
                        results.AddRange(deviceSteps.Get().Take(currentCount));
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
