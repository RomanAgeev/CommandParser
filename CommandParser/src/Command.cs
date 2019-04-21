using System;
using System.Linq;
using System.Collections.Generic;
using Guards;
using System.Dynamic;

namespace CommandParser {
    public class Command {
        readonly Action<ExpandoObject> action;        
        readonly List<(string, Section)> optionalSections = new List<(string, Section)>();
        (string, Section)? requiredSection;

        public Command(Action<ExpandoObject> action) {
            Guard.NotNull(action, nameof(action));

            this.action = action;
        }

        public Command Required(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            requiredSection = (name, sectionSetup(new Section()));
            return this;
        }

        public Command Optional(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            var section = (name, sectionSetup(new Section()));
            optionalSections.Add(section);
            return this;
        }

        public bool TryParse(string[] args, out Action action) {
            Guard.NotNull(args, nameof(args));

            var options = new ExpandoObject();
            var optionsDict = (IDictionary<string, object>)options;
            int offset = 0;            

            if(requiredSection.HasValue) {                
                (string name, Section section) = requiredSection.Value;

                if(section.TryParse(args, out ExpandoObject required)) {
                    optionsDict[name] = required;
                    offset += section.Length;                    
                }
                else {
                    action = null;
                    return false;                    
                }
            }

            ExpandoObject optional;
            var parsedSections = new List<(string, Section)>();
            do {
                optional = null;                
                string[] sectionArgs = args.Skip(offset).ToArray();

                foreach(var optionalSection in optionalSections.Except(parsedSections)) {
                    (string name, Section section) = optionalSection;                    

                    if(section.TryParse(sectionArgs, out optional)) {
                        optionsDict[name] = optional;
                        offset += section.Length;
                        parsedSections.Add(optionalSection);
                        break;
                    }
                }
            }
            while(optional != null && offset < args.Length);
            
            if(optionsDict.Count > 0) {                 
                action = () => this.action(options);
                return true;
            }
            else {
                action = null;
                return false;            
            }
        }
    }
}