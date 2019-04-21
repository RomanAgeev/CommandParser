using System;
using System.Linq;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using System.Dynamic;

namespace CommandParser.Tests {
    public class CommandTests {
        const string WRONG = "--WRONG";

        [Theory]
        [InlineData("--first", "First")]
        [InlineData("--first --WRONG", "First")]
        [InlineData("--first --second", "First,Second")]
        [InlineData("--first --WRONG --second", "First")]
        [InlineData("--first --second --WRONG", "First,Second")]
        [InlineData("--first --WRONG --third", "First")]
        [InlineData("--first --third --WRONG", "First,Third")]
        [InlineData("--first --second --third", "First,Second,Third")]
        [InlineData("--first --third --second", "First,Third,Second")]
        [InlineData("--first --second --third --WRONG", "First,Second,Third")]
        [InlineData("--first --second --WRONG --third", "First,Second")]
        [InlineData("--first --WRONG --second --third", "First")]
        public void NoParameters_WithRequiredSection_Success_Test(string args, string expectedOptions) {
            Assert_NoParameters_Success(true, args.Split(" "), expectedOptions.Split(","));
        }

        [Theory]
        [InlineData("--second", "Second")]
        [InlineData("--second --WRONG", "Second")]
        [InlineData("--third", "Third")]
        [InlineData("--third --WRONG", "Third")]
        [InlineData("--second --third", "Second,Third")]
        [InlineData("--third --second", "Third,Second")]
        [InlineData("--second --WRONG --third", "Second")]
        [InlineData("--second --third --WRONG", "Second,Third")]
        public void NoParameters_NoRequiredSection_Success_Test(string args, string expectedOptions) {         
            Assert_NoParameters_Success(false, args.Split(" "), expectedOptions.Split(","));
        }        

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(WRONG)]
        [InlineData(WRONG, "first")]
        [InlineData("second")]
        [InlineData("third")]
        [InlineData("second", "third")]
        public void NoParameters_WithRequiredSection_Fail_Test(params string[] args) {
            Assert_NoParameters_Fail(true, args);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(WRONG)]
        [InlineData(WRONG, "second")]
        [InlineData(WRONG, "third")]
        [InlineData(WRONG, "second", "third")]
        public void NoParameters_NoRequiredSection_Fail_Test(params string[] args) {
            Assert_NoParameters_Fail(false, args);
        }

        [Fact]
        public void WithParameters_Test() {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var args = new[] { "--firstKey", "A", "--secondKey", "B" };

            new Command(fakeOperation)
                .Required("First",
                    section => section
                        .WithKey("firstKey")
                        .WithString("FirstParam"))
                .Optional("Second",
                    section => section
                        .WithKey("secondKey")
                        .WithString("SecondParam"))
                .TryParse(args, out Action action)
                .Should().BeTrue();
            
            action.Should().NotBeNull();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).Invokes((ExpandoObject options) => {
                string firstParam = ((dynamic)options).First.FirstParam;
                string secondParam = ((dynamic)options).Second.SecondParam;

                firstParam.Should().Be("A");
                secondParam.Should().Be("B");
            });            

            action();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).MustHaveHappened();            
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Any")]
        public void EmptyCommand_Test(params string[] args) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            new Command(fakeOperation)
                .TryParse(args ?? new string[0], out Action action)
                .Should().BeFalse();

            action.Should().BeNull();            
        }

        void Assert_NoParameters_Success(bool withRequiredSection, string[] args, string[] expectedOptions) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var command = new Command(fakeOperation)
                .Optional("Second", section => section.WithKey("second"))
                .Optional("Third", section => section.WithKey("third"));

            if(withRequiredSection)
                command = command.Required("First", section => section.WithKey("first"));

            command.TryParse(args, out Action action)
                .Should().BeTrue();            

            action.Should().NotBeNull();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).Invokes((ExpandoObject options) => options
                .Select(x => x.Key).Should().Equal(expectedOptions));

            action();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).MustHaveHappened();
        }
        
        void Assert_NoParameters_Fail(bool withRequiredSection, string[] args) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var command = new Command(fakeOperation)
                .Optional("Second", section => section.WithKey("second"))
                .Optional("Third", section => section.WithKey("third"));

            if(withRequiredSection)
                command = command.Required("First", section => section.WithKey("first"));

            command.TryParse(args ?? new string[0], out Action action)
                .Should().BeFalse();            

            action.Should().BeNull();
        }
    }
}