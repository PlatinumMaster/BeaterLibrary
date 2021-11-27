using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BeaterLibrary.Formats.Scripts
{
    public class Command
    {
        public static List<CommandTypes> FunctionTypes = new()
            {CommandTypes.Call, CommandTypes.ConditionalJump, CommandTypes.Jump, CommandTypes.Actions};

        public Command(string Name, ushort ID, CommandTypes type, IReadOnlyList<Type> Types)
        {
            this.Name = Name;
            this.Types = Types;
            this.ID = ID;
            Type = type;
            Parameters = new List<object>();
        }

        public Command(string Name, ushort ID, CommandTypes type) : this(Name, ID, type, Array.Empty<Type>())
        {
        }

        public string Name { get; }
        public IReadOnlyList<Type> Types { get; }
        public CommandTypes Type { get; }
        public ushort ID { get; }
        public List<object> Parameters { get; }

        public int Size()
        {
            var Size = 0x2;
            foreach (var E in Types)
                Size += Marshal.SizeOf(E);
            return Size;
        }

        public override string ToString()
        {
            var FormattedParams = new string[Parameters.Count];
            for (var i = 0; i < Parameters.Count; ++i)
                FormattedParams[i] = Util.IsNumericType(Parameters[i])
                    ? $"0x{Parameters[i]:X}"
                    : Parameters[i].ToString();
            return string.Join(' ', Name, string.Join(", ", FormattedParams));
        }
    }
}