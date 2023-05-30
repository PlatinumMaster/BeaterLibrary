using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BeaterLibrary.Formats.Scripts {
    public class Command {
        public static List<CommandTypes> FunctionTypes = new() {
            CommandTypes.Call,
            CommandTypes.Jump,
            CommandTypes.ActionSequence
        };
        
        public Command(string name, ushort id, CommandTypes type, IReadOnlyList<Type> types,
            List<string> parameterNames, List<string> parameterDesc) {
            Name = name;
            Types = types;
            ParameterNames = parameterNames;
            ParameterDesc = parameterDesc;
            ID = id;
            Type = type;
            Parameters = new List<object>();
        }

        public Command(string name, ushort id, CommandTypes type) : this(name, id, type, Array.Empty<Type>(),
            new List<string>(), new List<string>()) {
        }

        public string Name { get; }
        public IReadOnlyList<Type> Types { get; }
        public CommandTypes Type { get; }
        public ushort ID { get; }
        public List<object> Parameters { get; }
        public List<string> ParameterNames { get; }
        public List<string> ParameterDesc { get; }
        public int Size() {
            var Size = 2;
            foreach (var e in Types)
                if (e == typeof(FX16))
                    Size += 2;
                else if (e == typeof(FX32))
                    Size += 4;
                else
                    Size += Marshal.SizeOf(e);
            return Size;
        }
        
        public override string ToString() {
            var FormattedParams = new string[Parameters.Count];
            for (var i = 0; i < Parameters.Count; i++)
                FormattedParams[i] = Util.isNumericType(Parameters[i])
                    ? string.Format("0x{0:X}", Parameters[i])
                    : Parameters[i].ToString();
            return string.Join(' ', Name, string.Join(", ", FormattedParams));
        }
    }
}