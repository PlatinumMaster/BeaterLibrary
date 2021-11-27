using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Trainer
{
    public class TrainerTextTable
    {
        public TrainerTextTable()
        {
            LookupTable = new Dictionary<ushort, List<TrainerTextTableEntry>>();
        }

        public TrainerTextTable(string Path) : this()
        {
            var Binary = new BinaryReader(File.OpenRead(Path));
            if (Binary.BaseStream.Length % 4 != 0)
                throw new Exception("Invalid Trainer Text");
            for (var i = 0; i < Binary.BaseStream.Length / 4; ++i)
            {
                var TID = Binary.ReadUInt16();
                if (!LookupTable.ContainsKey(TID))
                    LookupTable.Add(TID, new List<TrainerTextTableEntry>());
                LookupTable[TID].Add(new TrainerTextTableEntry
                {
                    TrainerID = TID,
                    MessageType = Binary.ReadUInt16()
                });
            }
        }

        private Dictionary<ushort, List<TrainerTextTableEntry>> LookupTable { get; }
    }

    public class TrainerTextTableEntry
    {
        public ushort TrainerID { get; set; }
        public ushort MessageType { get; set; }
    }
}