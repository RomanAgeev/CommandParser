using System;
using System.Linq;
using System.Collections.Generic;
using Guards;

namespace CommandParser {
    public class Command {
        readonly Action<IEnumerable<Option>> action;        
        readonly List<Section> optionalSections = new List<Section>();
        Section requiredSection;

        public Command(Action<IEnumerable<Option>> action) {
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

            var options = new List<Option>();
            int offset = 0;            

            if(requiredSection != null) {                
                if(requiredSection.TryParse(args, out Option required)) {
                    options.Add(required);
                    offset += requiredSection.Length;                    
                }
                else {
                    action = null;
                    return false;                    
                }
            }

            Option optional;
            List<Section> parsedSections = new List<Section>();
            do {
                optional = null;                
                string[] sectionArgs = args.Skip(offset).ToArray();
                foreach(var optionalSection in optionalSections.Except(parsedSections)) {
                    if(optionalSection.TryParse(sectionArgs, out optional)) {
                        options.Add(optional);
                        offset += optionalSection.Length;
                        parsedSections.Add(optionalSection);
                        break;
                    }
                }
            }
            while(optional != null && offset < args.Length);
            
            if(options.Count > 0) {                 
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