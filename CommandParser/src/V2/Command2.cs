using System;
using System.Collections.Generic;

namespace CommandParser.V2 {
    public class Command2 : Section2 {
        readonly Action<object> _action;
        readonly List<Section2> _sections = new List<Section2>();

        public List<Section2> Sections => _sections;

        public Command2(string name, Action<object> action)
            : base(name) {
            _action = action;
        }
        public Command2 Required(Func<Section2, Section2> setup) {
            setup(this);
            return this;
        }
        public Command2 Optional(string name, Func<Section2, Section2> setup) {
            _sections.Add(setup(new Section2(name)));
            return this;
        }
    }
}