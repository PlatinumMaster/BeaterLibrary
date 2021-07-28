using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeaterLibrary.Formats.Pokémon;

namespace BeaterLibrary.Formats.Scripts
{
    public class Actions
    {
        public List<Action> Commands { get; }
        public Actions(List<Action> Commands)
        {
            this.Commands = Commands;
        }

        public override string ToString() => Commands.Aggregate("", (Acc, E) => string.Join(' ', Acc, $"\t{E}\n"));
    }
}