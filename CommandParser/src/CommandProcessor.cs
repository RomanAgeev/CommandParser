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
                if(command.TryParse(args, out Action action))
                    return action;
            }
            throw new UnknownCommandException();            
        }
    }
}