using System.Collections.Generic;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts
{
    public class Actions
    {
        public Actions(List<Action> Commands)
        {
            this.Commands = Commands;
        }

        public List<Action> Commands { get; }

        public override string ToString()
        {
            return Commands.Aggregate("", (Acc, E) => string.Join(' ', Acc, $"\t{E}\n"));
        }
    }
}