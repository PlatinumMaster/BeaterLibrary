using System.Collections.Generic;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts
{
    public class ScriptMethod
    {
        public ScriptMethod(List<Command> Commands, int Address)
        {
            this.Commands = Commands;
            this.Address = Address;
        }

        public List<Command> Commands { get; set; }

        public int Address { get; set; }

        public override string ToString()
        {
            return Commands.Aggregate("", (x, y) => string.Join(' ', x, $"\t{y}\n"));
        }
    }

    public class AnonymousScriptMethod : ScriptMethod
    {
        public AnonymousScriptMethod(List<Command> Commands, int Address) : base(Commands, Address)
        {
        }
    }
}