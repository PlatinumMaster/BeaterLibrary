using System;
using System.IO;

namespace BeaterLibrary.Formats.Overworld {
    [Serializable]
    public class Trigger : FieldObject {
        public Trigger() {
            script = 0;
            valueNeededForExecution = 0;
            variable = 0;
            unknown = 0;
            unknown2 = 0;
            x = 0;
            y = 0;
            w = 0;
            h = 0;
            z = 0;
            unknown3 = 0;
        }

        public Trigger(BinaryReader b) {
            script = b.ReadUInt16();
            valueNeededForExecution = b.ReadUInt16();
            variable = b.ReadUInt16();
            unknown = b.ReadUInt16();
            unknown2 = b.ReadUInt16();
            x = b.ReadUInt16();
            y = b.ReadUInt16();
            w = b.ReadUInt16();
            h = b.ReadUInt16();
            z = b.ReadInt16();
            unknown3 = b.ReadUInt16();
        }

        public ushort script { get; set; }
        public ushort valueNeededForExecution { get; set; }
        public ushort variable { get; set; }

        public ushort unknown { get; set; }
        public ushort unknown2 { get; set; }
        public ushort x { get; set; }
        public ushort y { get; set; }
        public short z { get; set; }
        public ushort w { get; set; }
        public ushort h { get; set; }
        public ushort unknown3 { get; set; }

        public static uint size => 0x16;
    }
}