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
        static void Main(string[] args)
        {
            var inputParser = new InputParser<Numbers>(args);

            var config = Config.FromFile("config.json");
            var isCpu = config.IsCpu;

            var input = inputParser.FirstInput;
            List<int> results = null;

            if (isCpu)
            {
                results = CpuProcessor(input.Start, input.Count);
            }
            else
            {
                results = GpuCalculator.CalculateOnce(input.Start, input.Count, packetSize, maxSteps);
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
