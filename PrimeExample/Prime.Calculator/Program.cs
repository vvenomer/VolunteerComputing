using Prime.Shared;
using System;
using System.Linq;
using VolunteerComputing.ImplementationHelpers;

namespace Prime.Calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new InputParser<Numbers>(args);

            var numbers = inputParser.FirstInput;

            var result = Enumerable.Range(numbers.Start, numbers.Count)
                .AsParallel()
                .Select(i => IsPrime(i) ? 1 : 0)
                .Sum();

            new ResponseBuilder()
                .AddSingle(result)
                .Save(inputParser.OutputFile);
        }

        static bool IsPrime(int n)
        {
            if (n <= 3)
                return n > 1;
            if (n % 2 == 0 || n % 3 == 0)
                return false;
            for (var i = 5; i * i <= n; i += 6)
                if (n % i == 0 || n % (i + 2) == 0)
                    return false;
            return true;
        }
    }

}
