using System;
using System.IO;

namespace BeaterLibrary.Formats.Overworld
{
    [Serializable]
    public class Interactable
    {
        public Interactable()
        {
            Script = 0;
            Condition = 0;
            Interactibility = 0;
            RailIndex = 0;
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Interactable(BinaryReader b)
        {
            Script = b.ReadUInt16();
            Condition = b.ReadUInt16();
            Interactibility = b.ReadUInt16();
            RailIndex = b.ReadUInt16();
            X = b.ReadUInt32();
            Y = b.ReadUInt32();
            Z = b.ReadInt32() / 0x10;
        }

        public ushort Script { get; set; }
        public ushort Condition { get; set; }
        public ushort Interactibility { get; set; }
        public ushort RailIndex { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public int Z { get; set; }
        public static uint Size => 0x14;
    }
}