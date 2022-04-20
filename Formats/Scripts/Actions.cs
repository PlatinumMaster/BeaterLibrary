using System.Collections.Generic;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts {
    public class Actions {
        
        public List<Action> commands { get; }
        public Actions(List<Action> commands) {
            this.commands = commands;
        }


        public override string ToString() {
            return commands.Aggregate("", (acc, e) => string.Join(' ', acc, $"\t{e}\n"));
        }
    }
}