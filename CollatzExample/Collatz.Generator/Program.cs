using System.Collections.Generic;
using System.Linq;
using VolunteerComputing.ImplementationHelpers;
using Collatz.Shared;

namespace Collatz.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new InputParser<InputPacket>(args);


            var input = inputParser.FirstInput;

            var count = input.NextCount();
            int packets = input.NextPackets(count);
            System.Console.WriteLine($"count: {packets}, packets: {packets}");
            var numbers = Enumerable.Range(0, packets)
                .Select(i => new Numbers
                {
                    Start = input.Start + i * input.NumberOfResultsInPacket,
                    Count = input.NextResults(i, count, packets)
                });

            var newInput = (input.Start + count < input.End)
                ? new InputPacket
                {
                    Start = input.Start + count,
                    End = input.End,
                    NumberOfPackets = input.NumberOfPackets,
                    NumberOfResultsInPacket = input.NumberOfResultsInPacket
                }
                : null;

            new ResponseBuilder()
                .AddSingle(newInput)
                .AddMultiple(numbers)
                .Save(inputParser.OutputFile);
        }
    }
}
