using System.Collections.Generic;

namespace BeaterLibrary.Formats.Pokémon {
    internal class Move {
        public byte type { get; private set; }
        public byte effectCategory { get; private set; }
        public byte category { get; private set; }
        public byte power { get; private set; }
        public byte accuracy { get; private set; }
        public byte pp { get; private set; }
        public byte priority { get; private set; }
        public byte hits { get; private set; }
        public ushort resultEffect { get; private set; }
        public byte effectChance { get; private set; }
        public byte status { get; private set; }
        public byte mininumTurns { get; private set; }
        public byte maximumTurns { get; private set; }
        public byte critical { get; private set; }
        public byte flinch { get; private set; }
        public ushort effect { get; private set; }
        public byte hasRecoil { get; private set; }
        public byte isHealing { get; private set; }
        public byte target { get; private set; }
        public List<byte> stats { get; private set; }
        public List<byte> magnitudes { get; private set; }
        public List<byte> statChances { get; private set; }
        public byte flag { get; private set; }
    }
}