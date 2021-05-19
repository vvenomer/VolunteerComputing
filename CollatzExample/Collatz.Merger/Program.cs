using Collatz.Shared;
using System.Collections.Generic;
using System.Linq;
using VolunteerComputing.ImplementationHelpers;

namespace Collatz.Merger
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new InputParser<IEnumerable<Result>>(args);


            var results = inputParser.FirstInput;

            var first = results.First();

            foreach (var result in results.Skip(1))
            {
                first.Numbers.AddRange(result.Numbers);
                first.Steps.AddRange(result.Steps);
            }

            var ordered = first.Numbers
                .Zip(first.Steps)
                .OrderBy(x => x.First.Start)
                .ToList();

            var merged = new List<(Numbers First, IEnumerable<int> Second)>();
            merged.Add(ordered[0]);
            for (int i = 1; i < ordered.Count; i++)
            {
                var current = merged[^1];
                var next = ordered[i];
                if(current.First.Start + current.First.Count == next.First.Start)
                {
                    var newRange = new Numbers { Start = current.First.Start, Count = current.First.Count + next.First.Count };
                    merged[^1] = (newRange, current.Second.Concat(next.Second));
                }
                else
                {
                    merged.Add(next);
                }
            }

            new ResponseBuilder()
                .AddSingle(new Result { Numbers = merged.Select(x => x.First).ToList(), Steps = merged.Select(x => x.Second.ToList()).ToList() })
                .Save(inputParser.OutputFile);
        }
    }
}
