using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeaterLibrary.Formats.Furniture
{
    [Serializable()]
    public class NPC
    {
        public ushort ID { get; set; }
        public ushort ModelID { get; set; }
        public ushort MovementPermission { get; set; }
        public ushort Type { get; set; }
        public ushort SpawnFlag { get; set; }
        public ushort ScriptID { get; set; }
        public ushort FaceDirection { get; set; }
        public ushort SightRange { get; set; }
        public ushort Unknown { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort TraversalWidth { get; set; }
        public ushort TraversalHeight { get; set; }
        public ushort StartingX { get; set; }
        public ushort StartingY { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort Unknown3 { get; set; }
        public short Z { get; set; }
        public static uint Size => 0x24;

        public NPC()
        {
            ID = 0;
            ModelID = 0;
            MovementPermission = 0;
            Type = 0;
            SpawnFlag = 0;
            ScriptID = 0;
            FaceDirection = 0;
            SightRange = 0;
            Unknown = 0;
            Unknown2 = 0;
            TraversalHeight = 0;
            TraversalWidth = 0;
            StartingX = 0;
            StartingY = 0;
            X = 0;
            Y = 0;
            Unknown3 = 0;
            Z = 0;
        }

        public NPC(BinaryReader b)
        {
            ID = b.ReadUInt16();
            ModelID = b.ReadUInt16();
            MovementPermission = b.ReadUInt16();
            Type = b.ReadUInt16();
            SpawnFlag = b.ReadUInt16();
            ScriptID = b.ReadUInt16();
            FaceDirection = b.ReadUInt16();
            SightRange = b.ReadUInt16();
            Unknown = b.ReadUInt16();
            Unknown2 = b.ReadUInt16();
            TraversalHeight = b.ReadUInt16();
            TraversalWidth = b.ReadUInt16();
            StartingX = b.ReadUInt16();
            StartingY = b.ReadUInt16();
            X = b.ReadUInt16();
            Y = b.ReadUInt16();
            Unknown3 = b.ReadUInt16();
            Z = b.ReadInt16();
        }
    }
}