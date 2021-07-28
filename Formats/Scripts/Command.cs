using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BeaterLibrary.Formats.Scripts
{
    public class Command
    {
        public static List<CommandTypes> FunctionTypes = new List<CommandTypes>()
            {CommandTypes.Call, CommandTypes.ConditionalJump, CommandTypes.Jump, CommandTypes.Actions};
        public string Name { get; }
        public IReadOnlyList<Type> Types { get; }
        public CommandTypes Type { get; }
        public ushort ID { get; }
        public List<object> Parameters { get; }

        public int Size()
        {
            int Size = 0x2;
            foreach (Type E in Types)
                Size += Marshal.SizeOf(E);
            return Size;
        } 

        public Command(string Name, ushort ID, CommandTypes type, IReadOnlyList<Type> Types)
        {
            this.Name = Name;
            this.Types = Types;
            this.ID = ID;
            this.Type = type;
            Parameters = new List<object>();
        }

        public Command(string Name, ushort ID, CommandTypes type) : this(Name, ID, type, Array.Empty<Type>()) { }

        public override string ToString()
        {
            string[] FormattedParams = new string[Parameters.Count];
            for (int i = 0; i < Parameters.Count; ++i)
                FormattedParams[i] = Util.IsNumericType(Parameters[i]) ? $"0x{Parameters[i]:X}" : Parameters[i].ToString();
            return string.Join(' ', Name, string.Join(", ", FormattedParams)); 
        }
    }
}