using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace CommandParser {
    public class CommandProcessor {
        readonly List<Command> commands = new List<Command>();

        public Command Register(Action<ExpandoObject> action) {
            var command = new Command(action);
            commands.Add(command);
            return command;
        }

        public Action Parse(params string[] args) {
            foreach(var command in commands) {
                Action action = command.Parse(args);
                if(action != null)
                    return action;
            }
            return null;           
        }
    }
}