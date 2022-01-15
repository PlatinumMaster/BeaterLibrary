using System;
using System.IO;

namespace BeaterLibrary.Formats.Overworld {
    [Serializable]
    public class Warp : FieldObject {
        public Warp() {
            targetZone = 0;
            targetWarp = 0;
            contactDirection = 0;
            transitionType = 0;
            coordinateType = 0;
            x = 0;
            z = 0;
            y = 0;
            w = 0;
            h = 0;
            rail = 0;
        }

        public Warp(BinaryReader b) {
            targetZone = b.ReadUInt16();
            targetWarp = b.ReadUInt16();
            contactDirection = b.ReadByte();
            transitionType = b.ReadByte();
            coordinateType = b.ReadUInt16();
            x = b.ReadUInt16();
            z = b.ReadInt16();
            y = b.ReadUInt16();
            w = b.ReadUInt16();
            h = b.ReadUInt16();
            rail = b.ReadUInt16();
        }

        public ushort targetZone { get; set; }
        public ushort targetWarp { get; set; }
        public byte contactDirection { get; set; }
        public byte transitionType { get; set; }
        public ushort coordinateType { get; set; }
        public ushort x { get; set; }

        public short z { get; set; }
        public ushort y { get; set; }
        public ushort w { get; set; }
        public ushort h { get; set; }
        public ushort rail { get; set; }

        public static uint size => 0x14;
    }
}