using System.IO;

namespace BeaterLibrary.Formats.Scripts {
    public class InitializationScript {
        public InitializationScript() {
            type = 0;
            scriptIndex = 0;
            unknown = 0;
        }

        public InitializationScript(BinaryReader binary) {
            // Needs to be read from two sections.
            type = binary.ReadUInt16();
            scriptIndex = binary.ReadUInt16();
            unknown = binary.ReadUInt16();
        }

        public ushort type { get; set; }
        public ushort scriptIndex { get; set; }
        public ushort unknown { get; set; }

        public static uint size => 0x6;
    }
}