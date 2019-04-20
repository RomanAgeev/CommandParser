using System;
using System.Linq;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Xunit;
using System.Dynamic;

namespace CommandParser.Tests {
    public class CommandTests {
        const string WRONG = "WRONG";

        [Theory]
        [InlineData("first")]
        [InlineData("first", WRONG)]
        [InlineData("first", "second")]
        [InlineData("first", WRONG, "second")]
        [InlineData("first", "second", WRONG)]
        [InlineData("first", WRONG, "third")]
        [InlineData("first", "third", WRONG)]
        [InlineData("first", "second", "third")]
        [InlineData("first", "third", "second")]
        [InlineData("first", "second", "third", WRONG)]
        [InlineData("first", "second", WRONG, "third")]
        [InlineData("first", WRONG, "second", "third")]
        public void NoParameters_WithRequiredSection_Success_Test(params string[] args) {
            Assert_NoParameters_Success(true, args);
        }

        [Theory]
        [InlineData("second")]
        [InlineData("second", WRONG)]
        [InlineData("third")]
        [InlineData("third", WRONG)]
        [InlineData("second", "third")]
        [InlineData("third", "second")]
        [InlineData("second", WRONG, "third")]
        [InlineData("second", "third", WRONG)]
        public void NoParameters_NoRequiredSection_Success_Test(params string[] args) {         
            Assert_NoParameters_Success(false, args);
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

            var command = new Command(fakeOperation)
                .Required("FIRST",
                    section => section
                        .WithKeys("--firstKey")
                        .WithParameter("firstParam"))
                .Optional("SECOND",
                    section => section
                        .WithKeys("--secondKey")
                        .WithParameter("secondParam"));

            var args = new[] { "--firstKey", "A", "--secondKey", "B" };
            bool success = command.TryParse(args, out Action action);

            success.Should().BeTrue();            
            action.Should().NotBeNull();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).Invokes((ExpandoObject options) => {
                string firstParam = ((dynamic)options).FIRST.firstParam;
                string secondParam = ((dynamic)options).SECOND.secondParam;

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
            var command = new Command(fakeOperation);

            bool success = command.TryParse(args ?? new string[0], out Action action);

            success.Should().BeFalse();            
            action.Should().BeNull();            
        }

        void Assert_NoParameters_Success(bool withRequiredSection, string[] args) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var command = new Command(fakeOperation)
                .Optional("second", section => section.WithKeys("second"))
                .Optional("third", section => section.WithKeys("third"));

            if(withRequiredSection)
                command = command.Required("first", section => section.WithKeys("first"));

            bool success = command.TryParse(args, out Action action);

            success.Should().BeTrue();            
            action.Should().NotBeNull();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).Invokes((ExpandoObject options) => {
                var expected = new List<string>();
                foreach(var arg in args) {
                    if(arg != "WRONG")
                        expected.Add(arg);
                    else
                        break;
                }                

                options.Select(x => x.Key).Should().Equal(expected);                
            });            

            action();

            A.CallTo(() => fakeOperation.Invoke(A<ExpandoObject>._)).MustHaveHappened();
        }
        
        void Assert_NoParameters_Fail(bool withRequiredSection, string[] args) {
            var fakeOperation = A.Fake<Action<ExpandoObject>>();

            var command = new Command(fakeOperation)
                .Optional("second", section => section.WithKeys("second"))
                .Optional("third", section => section.WithKeys("third"));

            if(withRequiredSection)
                command = command.Required("first", section => section.WithKeys("first"));

            bool success = command.TryParse(args ?? new string[0], out Action action);

            success.Should().BeFalse();            
            action.Should().BeNull();
        }
    }
}