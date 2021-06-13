using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Formats.Text
{
    public class TrainerData
    {
        public byte Flag { get; private set; }
        public byte Class { get; private set; }
        public byte BattleType { get; private set; }
        public byte NumberOfPokémon { get; private set; }
        public List<ushort> Items { get; private set; }
        public uint AI { get; private set; }
        public byte BattleType2 { get; private set; }
        public TrainerData() { }

    }
}
