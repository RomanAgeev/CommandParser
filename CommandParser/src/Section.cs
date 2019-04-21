using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Guards;

namespace CommandParser {
    public class Section {
        const string LongKeyPrefix = "--";
        const string ShortKeyPrefix = "-";

        readonly List<(string, Func<string, object>)> _param = new List<(string, Func<string, object>)>();

        string _longKey;
        char? _shortKey;

        public int Length => _param.Count + 1;

        public Section WithKey(string longKey, char? shortKey = null) {
            Guard.NotNullOrWhiteSpace(longKey, nameof(longKey));
            
            _longKey = longKey;
            _shortKey = shortKey;
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
                if(args[0] == $"{LongKeyPrefix}{_longKey}" || (_shortKey.HasValue && args[0] == $"{ShortKeyPrefix}{_shortKey}")) {
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