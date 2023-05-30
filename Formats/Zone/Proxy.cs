using System;
using System.IO;

namespace BeaterLibrary.Formats.Zone_Entities {
    [Serializable]
    public class Proxy : FieldObject {
        public Proxy() {
            Script = 0;
            Condition = 0;
            Interactibility = 0;
            RailIndex = 0;
            x = 0;
            y = 0;
            z = 0;
        }

        public Proxy(BinaryReader b) {
            Script = b.ReadUInt16();
            Condition = b.ReadUInt16();
            Interactibility = b.ReadUInt16();
            RailIndex = b.ReadUInt16();
            x = b.ReadUInt32();
            y = b.ReadUInt32();
            z = b.ReadInt32() / 0x10;
        }

        public ushort Script { get; set; }
        public ushort Condition { get; set; }
        public ushort Interactibility { get; set; }
        public ushort RailIndex { get; set; }
        public uint x { get; set; }
        public uint y { get; set; }
        public int z { get; set; }
        public static uint Size => 0x14;
    }
}