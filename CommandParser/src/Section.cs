using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Guards;

namespace CommandParser {
    public class Section {
        readonly List<(string, Func<string, object>)> _param = new List<(string, Func<string, object>)>();

        string _key;

        public int Length => _param.Count + 1;

        public Section WithKey(string key) {
            Guard.NotNullOrWhiteSpace(key, nameof(key));
            
            _key = key;
            return this;
        }

        public Section WithString(string param) {
            Guard.NotNull(param, nameof(param));

            Func<string, object> parse = x => x;

            _param.Add((param, parse));
            return this;
        }

        public Section WithInteger(string param) {
            Guard.NotNull(param, nameof(param));

            Func<string, object> parse = x => int.TryParse(x, out int result) ? (int?)result : null;
            
            _param.Add((param, parse));
            return this;
        }

        public Section WithFlags<T>(string param) where T : struct {
            Guard.NotNull(param, nameof(param));

            Func<string, object> parse = x => Enum.TryParse<T>(x, true, out T result) ? (Nullable<T>)result : null;
            
            _param.Add((param, parse));
            return this;
        }

        public bool TryParse(string[] args, out ExpandoObject option) {
            Guard.NotNull(args, nameof(args));

            if(args.Length > _param.Count) {
                if(args[0] == _key) {
                   option = new ExpandoObject();
                   var optionDict = (IDictionary<string, object>)option;

                    for(int i = 0; i < _param.Count; i++) {
                        (string name, Func<string, object> parse) = _param[i];
                        optionDict[name] = parse(args[i + 1]);
                    }

                    return true;
                }
            }

            option = null;
            return false;
        }
    }
}