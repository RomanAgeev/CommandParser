using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandParser.V2 {
    public enum ParameterType { String, Number, Flags }

    public class Section2 {
        readonly string _name;
        readonly List<string> _keys = new List<string>();
        readonly List<(string, ParameterType)> _parameters = new List<(string, ParameterType)>();        

        public Section2(string name) {
            _name = name;
        }
        public Section2 WithKeys(params string[] keys) {
            _keys.AddRange(keys);
            return this;
        }
        public Section2 WithParameter(string paramName, ParameterType paramType) {
            _parameters.Add((paramName, paramType));
            return this;
        }
        public Regex CreateRegex() {
            var builder = new StringBuilder();

            builder.Append(_keys[0]);

            _parameters.Aggregate(builder, (bldr, parameter) => {
                var (_, paramType) = parameter;                
                switch(paramType) {
                    case ParameterType.String:
                        bldr.Append($@"\s*(\S+)\b");
                        break;
                    case ParameterType.Number:
                        bldr.Append($@"\s*(\d+)\b");
                        break;
                    case ParameterType.Flags:
                        bldr.Append($@"\s*([a-zA-Z\d]+(?:_[a-zA-Z\d]+)*)\b");
                        break;
                }
                return bldr;
            });
            return new Regex(builder.ToString(), RegexOptions.Compiled);
        }
    }
}