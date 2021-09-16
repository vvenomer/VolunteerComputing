using System;
using System.Collections.Generic;
using System.Linq;
using VolunteerComputing.ImplementationHelpers;

namespace Prime.Merger
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new InputParser<IEnumerable<int>>(args);

            var results = inputParser.FirstInput;

            var result = results.Sum();

            new ResponseBuilder()
                .AddSingle(result)
                .Save(inputParser.OutputFile);
        }
    }
}
