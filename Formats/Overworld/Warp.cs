using System;
using System.IO;

namespace BeaterLibrary.Formats.Overworld
{
    [Serializable]
    public class Warp
    {
        public Warp()
        {
            TargetZone = 0;
            TargetWarp = 0;
            ContactDirection = 0;
            TransitionType = 0;
            CoordinateType = 0;
            X = 0;
            Z = 0;
            Y = 0;
            W = 0;
            H = 0;
            Rail = 0;
        }

        public Warp(BinaryReader b)
        {
            TargetZone = b.ReadUInt16();
            TargetWarp = b.ReadUInt16();
            ContactDirection = b.ReadByte();
            TransitionType = b.ReadByte();
            CoordinateType = b.ReadUInt16();
            X = b.ReadUInt16();
            Z = b.ReadInt16();
            Y = b.ReadUInt16();
            W = b.ReadUInt16();
            H = b.ReadUInt16();
            Rail = b.ReadUInt16();
        }

        public ushort TargetZone { get; set; }
        public ushort TargetWarp { get; set; }
        public byte ContactDirection { get; set; }
        public byte TransitionType { get; set; }
        public ushort CoordinateType { get; set; }
        public ushort X { get; set; }
        
        public short Z { get; set; }
        public ushort Y { get; set; }
        public ushort W { get; set; }
        public ushort H { get; set; }
        public ushort Rail { get; set; }

        public static uint Size => 0x14;
    }
}