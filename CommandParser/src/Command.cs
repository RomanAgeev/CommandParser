using System;
using System.Linq;
using System.Collections.Generic;
using Guards;
using System.Dynamic;

namespace CommandParser {
    public class Command {
        readonly Action<ExpandoObject> action;        
        readonly List<(string, Section)> secondarySections = new List<(string, Section)>();
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

        public Command Secondary(string name, Func<Section, Section> sectionSetup) {
            Guard.NotNull(sectionSetup, nameof(sectionSetup));

            var section = (name, sectionSetup(new Section()));
            secondarySections.Add(section);
            return this;
        }

        public Action Parse(string[] args) {
            Guard.NotNull(args, nameof(args));

            var result = new ExpandoObject();
            var resultDict = (IDictionary<string, object>)result;
            int offset = 0;            

            // Parse primary section
            if(primarySection.HasValue) {                
                (string name, Section section) = primarySection.Value;

                var primary = section.Parse(args);
                if(primary != null) {
                    resultDict[name] = primary;
                    offset += section.Length;
                }
                else
                    return null;                
            }

            // Parse secondary sections
            ExpandoObject secondary;
            var parsedSections = new List<(string, Section)>();
            do {
                secondary = null;                
                string[] sectionArgs = args.Skip(offset).ToArray();

                foreach(var secondarySection in secondarySections.Except(parsedSections)) {
                    (string name, Section section) = secondarySection;                    

                    secondary = section.Parse(sectionArgs);                    
                    if(secondary != null) {
                        resultDict[name] = secondary;
                        offset += section.Length;
                        parsedSections.Add(secondarySection);
                        break;
                    }
                }
            }
            while(secondary != null && offset < args.Length);

            // Fill not parsed secondary sections by nulls
            foreach(var secondarySection in secondarySections.Except(parsedSections)) {
                (string name, _) = secondarySection;

                resultDict[name] = null;
            }
            
            if(offset > 0)
                return () => this.action(result);

            return null;
        }
    }
}