using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VolunteerComputing.ImplementationHelpers
{
    public class InputParser<T1>
    {
        public IList<string> Input { get; set; }
        public string OutputFile { get; set; }
        public T1 FirstInput => JsonConvert.DeserializeObject<T1>(Input[0]);

        public InputParser(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var inputFile = config["inputFile"];
            OutputFile = config["outputFile"];

            Input = JsonConvert.DeserializeObject<IList<string>>(File.ReadAllText(inputFile));
        }
    }

    public class InputParser<T1, T2> : InputParser<T1>
    {
        public InputParser(string[] args) : base(args)
        {
        }

        public T2 SecondInput => JsonConvert.DeserializeObject<T2>(Input[1]);
    }

    public class InputParser<T1, T2, T3> : InputParser<T1, T2>
    {
        public InputParser(string[] args) : base(args)
        {
        }

        public T3 ThirdInput => JsonConvert.DeserializeObject<T3>(Input[1]);
    }
}
