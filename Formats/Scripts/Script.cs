using System.Collections.Generic;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts {
    public class ScriptMethod {
        public ScriptMethod(List<Command> commands, int address) {
            this.Commands = commands;
            this.Address = address;
        }

        public List<Command> Commands { get; set; }

        public int Address { get; set; }

        public override string ToString() => Commands.Aggregate("", (x, y) => string.Join(' ', x, $"\t{y}\n"));
    }

    public class AnonymousScriptMethod : ScriptMethod {
        public AnonymousScriptMethod(List<Command> commands, int address) : base(commands, address) { }
    }
}