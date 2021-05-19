using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VolunteerComputing.ImplementationHelpers
{
    public class ResponseBuilder
    {
        List<IEnumerable<string>> output = new List<IEnumerable<string>>();

        public ResponseBuilder()
        {
        }

        public ResponseBuilder AddSingle<T>(T respone)
        {
            if (respone is not null)
                output.Add(new List<string>() { JsonConvert.SerializeObject(respone) });
            else
                output.Add(new List<string>());
            return this;
        }

        public ResponseBuilder AddMultiple<T>(IEnumerable<T> responses)
        {
            output.Add(responses.Select(n => JsonConvert.SerializeObject(n)));
            return this;
        }

        public ResponseBuilder AddEmpty()
        {
            output.Add(new List<string>());
            return this;
        }

        public void Save(string outputFile) => File.WriteAllText(outputFile, JsonConvert.SerializeObject(output));
    }
}
