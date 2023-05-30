using System.IO;

namespace BeaterLibrary.Formats.Zone_Entities {
    public class InitializationScript {
        public InitializationScript() {
            Type = 0;
            ScriptIndex = 0;
            Unknown = 0;
        }

        public InitializationScript(BinaryReader binary) {
            // Needs to be read from two sections.
            Type = binary.ReadUInt16();
            ScriptIndex = binary.ReadUInt16();
            Unknown = binary.ReadUInt16();
        }

        public ushort Type { get; set; }
        public ushort ScriptIndex { get; set; }
        public ushort Unknown { get; set; }

        public static uint Size => 0x6;
    }
}