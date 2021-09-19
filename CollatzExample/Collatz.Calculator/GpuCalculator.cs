using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
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
        static void Collatz(Index1 i, int start, int count, ArrayView<int> stepsArray, int max)
        {
			if (i < count)
			{
				int n = start + i;

				for (int steps = 0; steps < max; steps++)
				{
					if ((n & 1) == 1)
						n = (n << 1) + n + 1;
					else
						n >>= 1;

					if (n == 1)
					{
						stepsArray[i] = steps;
						return;
					}
				}
				stepsArray[i] = -1;
			}
		}

        public static List<int> CalculateOnce(int start, int count, int maxSize, int maxSteps)
        {
            while (true)
            {
                try
                {
                    using var context = new Context();
			        using var accelerator = new CudaAccelerator(context);
                    var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1, int, int, ArrayView<int>, int>(Collatz);

                    using var deviceSteps = accelerator.Allocate<int>(maxSize);
                    var results = new List<int>();
                    for (int i = 0; i < count; i += maxSize)
                    {
                        var c = count - i;
                        var currentCount = c > maxSize ? maxSize : c;
                        kernel(currentCount, start + i, currentCount, deviceSteps.View, maxSteps);
                        
			            accelerator.Synchronize();
                        results.AddRange(deviceSteps.GetAsArray().Take(currentCount));
                    }


                    /*var gpu = new Gpu();
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
                    }*/
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
