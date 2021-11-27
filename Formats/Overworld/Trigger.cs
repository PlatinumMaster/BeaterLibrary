using System;
using System.IO;

namespace BeaterLibrary.Formats.Overworld
{
    [Serializable]
    public class Trigger
    {
        public Trigger()
        {
            Script = 0;
            ValueNeededForExecution = 0;
            Variable = 0;
            Unknown = 0;
            Unknown2 = 0;
            X = 0;
            Y = 0;
            W = 0;
            H = 0;
            Z = 0;
            Unknown3 = 0;
        }

        public Trigger(BinaryReader b)
        {
            Script = b.ReadUInt16();
            ValueNeededForExecution = b.ReadUInt16();
            Variable = b.ReadUInt16();
            Unknown = b.ReadUInt16();
            Unknown2 = b.ReadUInt16();
            X = b.ReadUInt16();
            Y = b.ReadUInt16();
            W = b.ReadUInt16();
            H = b.ReadUInt16();
            Z = b.ReadInt16();
            Unknown3 = b.ReadUInt16();
        }

        public ushort Script { get; set; }
        public ushort ValueNeededForExecution { get; set; }
        public ushort Variable { get; set; }

        public ushort Unknown { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public short Z { get; set; }
        public ushort W { get; set; }
        public ushort H { get; set; }
        public ushort Unknown3 { get; set; }

        public static uint Size => 0x16;
    }
}