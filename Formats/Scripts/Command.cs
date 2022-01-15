using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BeaterLibrary.Formats.Scripts {
    public class Command {
        public static List<CommandTypes> functionTypes = new() {
            CommandTypes.Call,
            CommandTypes.Jump,
            CommandTypes.ActionSequence
        };
        
        public Command(string name, ushort id, CommandTypes type, IReadOnlyList<Type> types,
            List<string> parameterNames, List<string> parameterDesc) {
            this.name = name;
            this.types = types;
            this.parameterNames = parameterNames;
            this.parameterDesc = parameterDesc;
            this.id = id;
            this.type = type;
            parameters = new List<object>();
        }

        public Command(string name, ushort id, CommandTypes type) : this(name, id, type, Array.Empty<Type>(),
            new List<string>(), new List<string>()) {
        }

        public string name { get; }
        public IReadOnlyList<Type> types { get; }
        public CommandTypes type { get; }
        public ushort id { get; }
        public List<object> parameters { get; }
        public List<string> parameterNames { get; }
        public List<string> parameterDesc { get; }
        public int size() {
            var size = 2;
            foreach (var e in types)
                if (e == typeof(FX16))
                    size += 2;
                else if (e == typeof(FX32))
                    size += 4;
                else
                    size += Marshal.SizeOf(e);
            return size;
        }
        
        public override string ToString() {
            var formattedParams = new string[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
                formattedParams[i] = Util.isNumericType(parameters[i])
                    ? string.Format("0x{0:X}", parameters[i])
                    : parameters[i].ToString();
            return string.Join(' ', name, string.Join(", ", formattedParams));
        }
    }
}