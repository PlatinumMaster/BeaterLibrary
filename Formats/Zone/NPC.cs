using System;
using System.IO;

namespace BeaterLibrary.Formats.Zone_Entities {
    [Serializable]
    public class NPC : FieldObject {
        public NPC() {
            id = 0;
            modelId = 0;
            movementPermission = 0;
            type = 0;
            spawnFlag = 0;
            scriptId = 0;
            faceDirection = 0;
            parameter = 0;
            parameter2 = 0;
            parameter3 = 0;
            traversalHeight = 0;
            traversalWidth = 0;
            startingX = 0;
            startingY = 0;
            x = 0;
            z = 0;
            railSidePos = 0;
            y = 0;
        }

        public NPC(BinaryReader b) {
            id = b.ReadUInt16();
            modelId = b.ReadUInt16();
            movementPermission = b.ReadUInt16();
            type = b.ReadUInt16();
            spawnFlag = b.ReadUInt16();
            scriptId = b.ReadUInt16();
            faceDirection = b.ReadUInt16();
            parameter = b.ReadUInt16();
            parameter2 = b.ReadUInt16();
            parameter3 = b.ReadUInt16();
            traversalWidth = b.ReadUInt16();
            traversalHeight = b.ReadUInt16();
            isRailSys = b.ReadInt32() is 1;
            x = b.ReadUInt16();
            y = b.ReadUInt16();
            railSidePos = b.ReadUInt16();
            z = b.ReadInt16();
        }


        public ushort id { get; set; }
        public ushort modelId { get; set; }
        public ushort movementPermission { get; set; }
        public ushort type { get; set; }
        public ushort spawnFlag { get; set; }
        public ushort scriptId { get; set; }
        public ushort faceDirection { get; set; }
        public ushort parameter { get; set; }
        public ushort parameter2 { get; set; }
        public ushort parameter3 { get; set; }
        public ushort traversalWidth { get; set; }
        public ushort traversalHeight { get; set; }
        public ushort startingX { get; set; }
        public ushort startingY { get; set; }
        public bool isRailSys { get; set; }
        public ushort x { get; set; }
        public short z { get; set; }
        public ushort railSidePos { get; set; }
        public ushort y { get; set; }
        public static uint size => 0x24;
    }
}