using System.IO;

namespace BeaterLibrary.Formats.Scripts
{
    public class InitializationScript
    {
        public InitializationScript()
        {
            Type = 0;
            ScriptIndex = 0;
            Unknown = 0;
        }

        public InitializationScript(BinaryReader Binary)
        {
            // Needs to be read from two sections.
            Type = Binary.ReadUInt16();
            ScriptIndex = Binary.ReadUInt16();
            Unknown = Binary.ReadUInt16();
        }

        public ushort Type { get; set; }
        public ushort ScriptIndex { get; set; }
        public ushort Unknown { get; set; }

        public static uint Size => 0x6;
    }
}