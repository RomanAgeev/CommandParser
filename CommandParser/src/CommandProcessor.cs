using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Guards;

namespace CommandParser {
    public class CommandProcessor {
        readonly List<Command> commands = new List<Command>();

        public Command Register(Action<ExpandoObject> action) {
            Guard.NotNull(action, nameof(action));

            var command = new Command(action);
            commands.Add(command);
            return command;
        }

        public Action Parse(params string[] args) {
            Guard.NotNull(args, nameof(args));

            foreach(var command in commands) {
                Action action = command.Parse(args);
                if(action != null)
                    return action;
            }
            return null;           
        }
    }
}