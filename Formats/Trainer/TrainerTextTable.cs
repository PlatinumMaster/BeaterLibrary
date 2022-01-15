using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Trainer {
    public class TrainerTextTable {
        public TrainerTextTable() {
            lookupTable = new Dictionary<ushort, List<TrainerTextTableEntry>>();
        }

        public TrainerTextTable(string path) : this() {
            var binary = new BinaryReader(File.OpenRead(path));
            if (binary.BaseStream.Length % 4 != 0)
                throw new Exception("Invalid Trainer Text");
            for (var i = 0; i < binary.BaseStream.Length / 4; ++i) {
                var tid = binary.ReadUInt16();
                if (!lookupTable.ContainsKey(tid))
                    lookupTable.Add(tid, new List<TrainerTextTableEntry>());
                lookupTable[tid].Add(new TrainerTextTableEntry {
                    trainerId = tid,
                    messageType = binary.ReadUInt16()
                });
            }
        }

        private Dictionary<ushort, List<TrainerTextTableEntry>> lookupTable { get; }
    }

    public class TrainerTextTableEntry {
        public ushort trainerId { get; set; }
        public ushort messageType { get; set; }
    }
}