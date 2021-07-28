using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Formats.Pokémon
{
    class Move
    {
        public byte Type { get; private set; }
        public byte EffectCategory { get; private set; }
        public byte Category { get; private set; }
        public byte Power { get; private set; }
        public byte Accuracy { get; private set; }
        public byte PP { get; private set; }
        public byte Priority { get; private set; }
        public byte Hits { get; private set; }
        public ushort ResultEffect { get; private set; }
        public byte EffectChance { get; private set; }
        public byte Status { get; private set; }
        public byte MininumTurns { get; private set; }
        public byte MaximumTurns { get; private set; }
        public byte Critical { get; private set; }
        public byte Flinch { get; private set; }
        public ushort Effect { get; private set; }
        public byte HasRecoil { get; private set; }
        public byte IsHealing { get; private set; }
        public byte Target { get; private set; }
        public List<byte> Stats { get; private set; }
        public List<byte> Magnitudes { get; private set; }
        public List<byte> StatChances { get; private set; }
        public byte Flag { get; private set; }
    }
}