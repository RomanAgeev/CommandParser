using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Guards;

namespace CommandParser {
    public class Section {
        readonly string _name;
        readonly List<string> _keys = new List<string>();
        readonly List<string> _param = new List<string>();

        public int Length => _param.Count + 1;
        public string Name => _name;

        public Section(string name) {
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            _name = name;
        }

        public Section WithKeys(params string[] keys) {
            Guard.NotNullOrEmpty(keys, nameof(keys));

            _keys.AddRange(keys);
            return this;
        }

        public Section WithParameter(string param) {
            Guard.NotNull(param, nameof(param));

            _param.Add(param);
            return this;
        }

        public bool TryParse(string[] args, out ExpandoObject option) {
            Guard.NotNull(args, nameof(args));

            if(args.Length >= _param.Count + 1) {
                if(_keys.Contains(args[0])) {
                   option = new ExpandoObject();
                   var optionDict = (IDictionary<string, object>)option;
                    for(int i = 0; i < _param.Count; i++)
                        optionDict[_param[i]] = args[i + 1];

                    return true;
                }
            }

            option = null;
            return false;
        }
    }
}