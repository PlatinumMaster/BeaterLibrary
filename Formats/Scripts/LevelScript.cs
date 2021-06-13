using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeaterLibrary.Formats.Scripts
{
    public class LevelScriptDeclaration
    {
        public ushort Unknown { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }
        public LevelScriptData Data { get; set; }
        public static uint Size { get => 0x6; }

        public LevelScriptDeclaration()
        {
            Unknown = 0;
            Unknown2 = 0;
            Unknown3 = 0;
        }

        public LevelScriptDeclaration(BinaryReader Binary)
        {
            // Needs to be read from two sections.
            Unknown = Binary.ReadUInt16();
            Unknown2 = Binary.ReadUInt16();
            Unknown3 = Binary.ReadUInt16();
        }
    }
    public class LevelScriptData
    {
        public ushort Unknown { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }

        public LevelScriptData()
        {
            Unknown = 0;
            Unknown2 = 0;
            Unknown3 = 0;
        }

        public LevelScriptData(BinaryReader Binary)
        {
            // Needs to be read from two sections.
            Unknown = Binary.ReadUInt16();
            Unknown2 = Binary.ReadUInt16();
            Unknown3 = Binary.ReadUInt16();
        }
    }
}
