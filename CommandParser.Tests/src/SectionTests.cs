using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using FluentAssertions;
using Xunit;
using System;

namespace CommandParser.Tests {
    public class SectionTests {
        [Flags]
        public enum TestFlags {
            Flag1 = 1 << 0,
            Flag2 = 1 << 1
        }

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
        [InlineData("key")]
        [InlineData("key", "abc", " ")]
        public void Parse_NoParams_Test(params string[] args) {
            new Section().WithKey("key")
                .Parse(args)
                .Should().NotBeNull();
        }

        [Theory]
        [InlineData("key", "A", "B")]
        [InlineData("key", "A", "B", "abc", " ")]
        public void Parse_WithParams_Test(params string[] args) {
            var option = new Section()
                .WithKey("key")
                .WithString("X")
                .WithString("Y")
                .Parse(args);

            option.Should().NotBeNull();

            var optionDict = (IDictionary<string, object>)option;
            optionDict["X"].Should().Be("A");
            optionDict["Y"].Should().Be("B");
        }

        [Theory]
        [InlineData("wrong-key", "A", "B")]
        [InlineData("key", "A")]
        [InlineData("")]
        public void Parse_Wrong_Args_Test(params string[] args) {
            new Section()
                .WithKey("key")
                .WithString("X")
                .WithString("Y")
                .Parse(args)
                .Should().BeNull();
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("A", null)]
        [InlineData("", null)]
        public void Parse_Integer(string arg, int? expected) {
            var option = new Section()
                .WithKey("key")
                .WithInteger("X")
                .Parse(new[] { "key", arg });

            option.Should().NotBeNull();

            var optionDict = (IDictionary<string, object>)option;
            optionDict["X"].Should().Be(expected);
        }

        [Theory]
        [InlineData("flag1", TestFlags.Flag1)]
        [InlineData("flag2", TestFlags.Flag2)]
        [InlineData("flag1,flag2", TestFlags.Flag1 | TestFlags.Flag2)]
        [InlineData("flag2,flag1", TestFlags.Flag1 | TestFlags.Flag2)]
        [InlineData("flag1_flag2", null)]
        [InlineData("WRONG", null)]
        [InlineData("", null)]
        public void Parse_Flags(string arg, TestFlags? expected) {
            var option = new Section()
                .WithKey("key")
                .WithFlags<TestFlags>("X")
                .Parse(new[] { "key", arg });

            option.Should().NotBeNull();

            var optionDict = (IDictionary<string, object>)option;
            optionDict["X"].Should().Be(expected);
        }
    }
}