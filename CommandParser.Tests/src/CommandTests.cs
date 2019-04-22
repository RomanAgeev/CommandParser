using System;
using System.Linq;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using System.Dynamic;

namespace CommandParser.Tests {
    public class CommandTests {
        [Theory]
        [InlineData("first", "First")]
        [InlineData("first WRONG", "First")]
        [InlineData("first second", "First,Second")]
        [InlineData("first WRONG second", "First")]
        [InlineData("first second WRONG", "First,Second")]
        [InlineData("first WRONG third", "First")]
        [InlineData("first third WRONG", "First,Third")]
        [InlineData("first second third", "First,Second,Third")]
        [InlineData("first third second", "First,Third,Second")]
        [InlineData("first second third WRONG", "First,Second,Third")]
        [InlineData("first second WRONG third", "First,Second")]
        [InlineData("first WRONG second third", "First")]
        public void NoParameters_WithPrimarySection_Success_Test(string args, string expectedOptions) {
            Assert_NoParameters_Success(true, args.Split(" "), expectedOptions.Split(","));
        }

        [Theory]
        [InlineData("second", "Second")]
        [InlineData("second WRONG", "Second")]
        [InlineData("third", "Third")]
        [InlineData("third WRONG", "Third")]
        [InlineData("second third", "Second,Third")]
        [InlineData("third second", "Third,Second")]
        [InlineData("second WRONG third", "Second")]
        [InlineData("second third WRONG", "Second,Third")]
        public void NoParameters_NoPrimarySection_Success_Test(string args, string expectedOptions) {         
            Assert_NoParameters_Success(false, args.Split(" "), expectedOptions.Split(","));
        }        

        [Theory]
        [InlineData("")]
        [InlineData("WRONG")]
        [InlineData("WRONG first")]
        [InlineData("second")]
        [InlineData("third")]
        [InlineData("second third")]
        public void NoParameters_WithPrimarySection_Fail_Test(string args) {
            Assert_NoParameters_Fail(true, args.Split(" "));
        }

        [Theory]
        [InlineData("")]
        [InlineData("WRONG")]
        [InlineData("WRONG second")]
        [InlineData("WRONG third")]
        [InlineData("WRONG second third")]
        public void NoParameters_NoPrimarySection_Fail_Test(string args) {
            Assert_NoParameters_Fail(false, args.Split(" "));
        }

        [Fact]
        public void WithParameters_Test() {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var args = new[] { "firstKey", "A", "secondKey", "B" };

            Action action = new Command(fakeOperation)
                .Primary("First",
                    section => section
                        .WithKey("firstKey")
                        .WithString("FirstParam"))
                .Optional("Second",
                    section => section
                        .WithKey("secondKey")
                        .WithString("SecondParam"))
                .Parse(args);
            
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
                .Parse(args ?? new string[0])
                .Should().BeNull();
        }

        void Assert_NoParameters_Success(bool withPrimarySection, string[] args, string[] expectedOptions) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var command = new Command(fakeOperation)
                .Optional("Second", section => section.WithKey("second"))
                .Optional("Third", section => section.WithKey("third"));

            if(withPrimarySection)
                command = command.Primary("First", section => section.WithKey("first"));

            Action action = command.Parse(args);

            action.Should().NotBeNull();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).Invokes((ExpandoObject options) => options
                .Select(x => x.Key).Should().Equal(expectedOptions));

            action();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).MustHaveHappened();
        }
        
        void Assert_NoParameters_Fail(bool withPrimarySection, string[] args) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var command = new Command(fakeOperation)
                .Optional("Second", section => section.WithKey("second"))
                .Optional("Third", section => section.WithKey("third"));

            if(withPrimarySection)
                command = command.Primary("First", section => section.WithKey("first"));

            command.Parse(args ?? new string[0])
                .Should().BeNull();
        }
    }
}