using System;
using System.Linq;
using System.Collections.Generic;
using Guards;
using System.Dynamic;

namespace CommandParser {
    public class Command {
        readonly Action<ExpandoObject> action;        
        readonly List<Section> optionalSections = new List<Section>();
        Section requiredSection;

        public Command(Action<ExpandoObject> action) {
            Guard.NotNull(action, nameof(action));

            this.action = action;
        }

        public Command Required(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            requiredSection = sectionSetup(new Section(name));
            return this;
        }

        public Command Optional(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            var section = sectionSetup(new Section(name));
            optionalSections.Add(section);
            return this;
        }

        public bool TryParse(string[] args, out Action action) {
            Guard.NotNull(args, nameof(args));

            var options = new ExpandoObject();
            var optionsDict = (IDictionary<string, object>)options;
            int offset = 0;            

            if(requiredSection != null) {                
                if(requiredSection.TryParse(args, out ExpandoObject required)) {
                    optionsDict[requiredSection.Name] = required;
                    offset += requiredSection.Length;                    
                }
                else {
                    action = null;
                    return false;                    
                }
            }

            ExpandoObject optional;
            List<Section> parsedSections = new List<Section>();
            do {
                optional = null;                
                string[] sectionArgs = args.Skip(offset).ToArray();
                foreach(var optionalSection in optionalSections.Except(parsedSections)) {
                    if(optionalSection.TryParse(sectionArgs, out optional)) {
                        optionsDict[optionalSection.Name] = optional;
                        offset += optionalSection.Length;
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