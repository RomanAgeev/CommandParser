using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using FluentAssertions;
using Xunit;

namespace CommandParser.Tests {
    public class SectionTests {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public void Length_Test(int paramCount) {
            Enumerable
                .Range(1, paramCount)
                .Select(i => $"param{i}")
                .Aggregate(new Section().WithKey("key"), (section, param) => section.WithString(param))
                .Length
                    .Should().Be(paramCount + 1);
        }

        [Theory]        
        [InlineData("-k")]
        [InlineData("--key")]
        [InlineData("--key", "abc", " ")]
        public void TryParse_NoParams_Success_Test(params string[] args) {
            new Section().WithKey("key", 'k')
                .TryParse(args, out ExpandoObject option)
                .Should().BeTrue();

            option.Should().NotBeNull();
        }

        [Theory]
        [InlineData("--key", "1", "A")]
        [InlineData("--key", "1", "A", "abc", " ")]
        public void TryParse_WithParams_Success_Test(params string[] args) {
            new Section()
                .WithKey("key")
                .WithInteger("X")
                .WithString("Y")
                .TryParse(args, out ExpandoObject option)
                .Should().BeTrue();

            option.Should().NotBeNull();

            var optionDict = (IDictionary<string, object>)option;
            optionDict["X"].Should().Be(1);
            optionDict["Y"].Should().Be("A");
        }

        [Theory]
        [InlineData("--wrong-key", "1", "A")]
        [InlineData("--key", "1")]
        [InlineData("")]
        public void TryParse_Fail_Test(params string[] args) {
            new Section()
                .WithKey("--key")
                .WithInteger("X")
                .WithString("Y")
                .TryParse(args, out ExpandoObject option)
                .Should().BeFalse();

            option.Should().BeNull();            
        }
    }
}