using System;
using System.Linq;
using System.Collections.Generic;
using Guards;
using System.Dynamic;

namespace CommandParser {
    public class Command {
        readonly Action<ExpandoObject> action;        
        readonly List<(string, Section)> optionalSections = new List<(string, Section)>();
        (string, Section)? primarySection;

        public Command(Action<ExpandoObject> action) {
            Guard.NotNull(action, nameof(action));

            this.action = action;
        }

        public Command Primary(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            primarySection = (name, sectionSetup(new Section()));
            return this;
        }

        public Command Optional(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            var section = (name, sectionSetup(new Section()));
            optionalSections.Add(section);
            return this;
        }

        public Action Parse(string[] args) {
            Guard.NotNull(args, nameof(args));

            var options = new ExpandoObject();
            var optionsDict = (IDictionary<string, object>)options;
            int offset = 0;            

            if(primarySection.HasValue) {                
                (string name, Section section) = primarySection.Value;

                var primary = section.Parse(args);
                if(primary != null) {
                    optionsDict[name] = primary;
                    offset += section.Length;
                }
                else
                    return null;                
            }

            ExpandoObject optional;
            var parsedSections = new List<(string, Section)>();
            do {
                optional = null;                
                string[] sectionArgs = args.Skip(offset).ToArray();

                foreach(var optionalSection in optionalSections.Except(parsedSections)) {
                    (string name, Section section) = optionalSection;                    

                    optional = section.Parse(sectionArgs);                    
                    if(optional != null) {
                        optionsDict[name] = optional;
                        offset += section.Length;
                        parsedSections.Add(optionalSection);
                        break;
                    }
                }
            }
            while(optional != null && offset < args.Length);
            
            if(optionsDict.Count > 0)
                return () => this.action(options);

            return null;
        }
    }
}