using System.Text.RegularExpressions;
using CommandParser.V2;
using Xunit;

namespace CommandParser.Tests.V2 {
    public class V2Tests {
        [Fact]
        public void ParseCommand() {
            var command = new Command2("Compress", obj => {})
                .Required(it => it
                    .WithKeys("compress")
                    .WithParameter("Src", ParameterType.String)
                    .WithParameter("Dest", ParameterType.String))
                .Optional("JobCount", it => it
                    .WithKeys("--job-count")
                    .WithParameter("Value", ParameterType.Number))
                .Optional("ProfilePipeline", it => it
                    .WithKeys("--profile-pipeline")
                    .WithParameter("Value", ParameterType.Flags));

            const string input = "compress ./1.jpg      --job-count     33333344444  --profile-pipeline A_B_C_D";
            
            var result = command.CreateRegex().Match(input);
            var result2 = command.Sections[0].CreateRegex().Match(input);
            var result3 = command.Sections[1].CreateRegex().Match(input);
        }        
    }
}