using System.Collections.Generic;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts {
    public class ScriptMethod {
        public ScriptMethod(List<Command> commands, int address) {
            this.commands = commands;
            this.address = address;
        }

        public List<Command> commands { get; set; }

        public int address { get; set; }

        public override string ToString() => commands.Aggregate("", (x, y) => string.Join(' ', x, $"\t{y}\n"));
    }

    public class AnonymousScriptMethod : ScriptMethod {
        public AnonymousScriptMethod(List<Command> commands, int address) : base(commands, address) { }
    }
}