using System;
using System.IO;

namespace BeaterLibrary.Formats.Overworld {
    [Serializable]
    public class Interactable : FieldObject {
        public Interactable() {
            script = 0;
            condition = 0;
            interactibility = 0;
            railIndex = 0;
            x = 0;
            y = 0;
            z = 0;
        }

        public Interactable(BinaryReader b) {
            script = b.ReadUInt16();
            condition = b.ReadUInt16();
            interactibility = b.ReadUInt16();
            railIndex = b.ReadUInt16();
            x = b.ReadUInt32();
            y = b.ReadUInt32();
            z = b.ReadInt32() / 0x10;
        }

        public ushort script { get; set; }
        public ushort condition { get; set; }
        public ushort interactibility { get; set; }
        public ushort railIndex { get; set; }
        public uint x { get; set; }
        public uint y { get; set; }
        public int z { get; set; }
        public static uint size => 0x14;
    }
}