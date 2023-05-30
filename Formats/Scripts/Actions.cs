using System.Collections.Generic;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts {
    public class Actions {
        
        public List<Action> Commands { get; }
        public Actions(List<Action> commands) {
            Commands = commands;
        }


        public override string ToString() {
            return Commands.Aggregate("", (acc, e) => string.Join(' ', acc, $"\t{e}\n"));
        }
    }
}