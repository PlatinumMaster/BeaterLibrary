using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeaterLibrary.Formats.Scripts
{
    public class InitializationScript
    {
        public ushort Unknown { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }
        public InitializationScriptSecondaryData SecondaryData { get; set; }

        public static uint Size => 0x6;

        public InitializationScript()
        {
            Unknown = 0;
            Unknown2 = 0;
            Unknown3 = 0;
        }

        public InitializationScript(BinaryReader Binary)
        {
            // Needs to be read from two sections.
            Unknown = Binary.ReadUInt16();
            Unknown2 = Binary.ReadUInt16();
            Unknown3 = Binary.ReadUInt16();
        }
    }

    public class InitializationScriptSecondaryData
    {
        public ushort Unknown { get; set; }
        public ushort Unknown2 { get; set; }
        public ushort Unknown3 { get; set; }

        public InitializationScriptSecondaryData()
        {
            Unknown = 0;
            Unknown2 = 0;
            Unknown3 = 0;
        }

        public InitializationScriptSecondaryData(BinaryReader Binary)
        {
            // Needs to be read from two sections.
            Unknown = Binary.ReadUInt16();
            Unknown2 = Binary.ReadUInt16();
            Unknown3 = Binary.ReadUInt16();
        }
    }
}