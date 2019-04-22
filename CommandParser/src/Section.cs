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

        public Section WithString(string name) {
            Guard.NotNull(name, nameof(name));

            Func<string, object> parse = x => x;

            _param.Add((name, parse));
            return this;
        }

        public Section WithInteger(string name) {
            Guard.NotNull(name, nameof(name));

            Func<string, object> parse = x => int.TryParse(x, out int result) ? (int?)result : null;
            
            _param.Add((name, parse));
            return this;
        }

        public Section WithFlags<T>(string name) where T : struct {
            Guard.NotNull(name, nameof(name));

            Func<string, object> parse = x => Enum.TryParse<T>(x, true, out T result) ? (Nullable<T>)result : null;
            
            _param.Add((name, parse));
            return this;
        }

        public ExpandoObject Parse(string[] args) {
            Guard.NotNull(args, nameof(args));

            if(args.Length > _param.Count && args[0] == _key) {
                var result = new ExpandoObject();
                var resultDict = (IDictionary<string, object>)result;

                for(int i = 0; i < _param.Count; i++) {
                    (string name, Func<string, object> parse) = _param[i];
                    resultDict[name] = parse(args[i + 1]);
                }

                return result;
            }

            return null;
        }
    }
}