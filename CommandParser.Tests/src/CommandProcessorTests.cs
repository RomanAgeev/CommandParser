using Xunit;
using FakeItEasy;
using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Dynamic;

namespace CommandParser.Tests {
    public class CommandProccessorTests {
        readonly CommandProcessor processor = new CommandProcessor();
        readonly Action<ExpandoObject> operation1 = A.Fake<Action<ExpandoObject>>();
        readonly Action<ExpandoObject> operation2 = A.Fake<Action<ExpandoObject>>();

        public CommandProccessorTests() {
            processor.Register(operation1)
                .Primary("Operation1", section => section
                    .WithKey("operation1"));

            processor.Register(operation2)
                .Primary("Operation2", section => section
                    .WithKey("operation2"));
        }

        [Fact]
        public void Parse_Success_Test() {
            var action1 = processor.Parse(new[] { "operation1" });
            var action2 = processor.Parse(new[] { "operation2" });
            action1.Should().NotBeNull();
            action2.Should().NotBeNull();

            action1();
            action2();

            A.CallTo(() => operation1.Invoke(A<ExpandoObject>._)).MustHaveHappened();
            A.CallTo(() => operation2.Invoke(A<ExpandoObject>._)).MustHaveHappened();
        }

        [Theory]
        [InlineData("WRONG")]
        [InlineData("")]
        public void Parse_Fail_Test(string arg) {
            processor.Parse(new[] { arg })
                .Should().BeNull();
        }
    }
}
