using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Algorithms;
using Prime.Shared;
using VolunteerComputing.ImplementationHelpers;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;

namespace Prime.Calculator
{
    class Program
    {
        const int packetSize = 256 * 256;

        static void IsPrime(Index1 i, int start, int count, ArrayView<int> stepsArray)
        {
            if (i < count)
            {
                int n = start + i;
                if (n <= 3)
                    stepsArray[i] = n > 1 ? 1 : 0;
                if (n % 2 == 0 || n % 3 == 0)
                    stepsArray[i] = 0;
                for (var j = 5; j * j <= n; j += 6)
                    if (n % j == 0 || n % (j + 2) == 0)
                        stepsArray[i] = 0;
                stepsArray[i] = 1;
            }
        }

        static void Main(string[] args)
        {
            var inputParser = new InputParser<Numbers>(args);

            var numbers = inputParser.FirstInput;

            var result = 0;

            {
                using var context = new Context();

                context.EnableAlgorithms();

                using var accelerator = new CudaAccelerator(context);

                var kernel = accelerator.LoadAutoGroupedStreamKernel<Index1, int, int, ArrayView<int>>(IsPrime);

                using var buffer = accelerator.Allocate<int>(packetSize);

                for (int i = 0; i < numbers.Count; i += packetSize)
                {
                    var c = numbers.Count - i;
                    var currentCount = c > packetSize ? packetSize : c;
                    kernel(currentCount, numbers.Start + i, currentCount, buffer.View);

                    using var resultBuffer = accelerator.Allocate<int>(1);

                    accelerator.Synchronize();

                    accelerator.Reduce<int, AddInt32>(accelerator.DefaultStream, buffer, resultBuffer);

                    accelerator.Synchronize();

                    result += resultBuffer.GetAsArray()[0];
                }
            }

            new ResponseBuilder()
                .AddSingle(result)
                .Save(inputParser.OutputFile);
        }
    }

}
