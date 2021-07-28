using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Formats.Pokémon
{
    class Pokédex
    {
        public byte BaseHP { get; private set; }
        public byte BaseAttack { get; private set; }
        public byte BaseDefense { get; private set; }
        public byte BaseSpeed { get; private set; }
        public byte BaseSpecialAttack { get; private set; }
        public byte BaseSpecialDefense { get; private set; }
        public byte PrimaryType { get; private set; }
        public byte SecondaryType { get; private set; }
        public byte CatchRate { get; private set; }
        public byte Stage { get; private set; }
        public ushort EVs { get; private set; }
        public List<ushort> Items { get; private set; }
        public byte Gender { get; private set; }
        public byte HatchCycle { get; private set; }
        public byte BaseHappiness { get; private set; }
        public byte ExperienceRate { get; private set; }
        public byte EggGroup { get; private set; }
        public byte EggGroup2 { get; private set; }
        public byte PrimaryAbility { get; private set; }
        public byte SecondaryAbility { get; private set; }
        public byte TertiaryAbility { get; private set; }
        public byte CanFlee { get; private set; }
        public ushort FormID { get; private set; }
        public ushort Form { get; private set; }
        public byte NumberOfForms { get; private set; }
        public byte Color { get; private set; }
        public ushort BaseExperience { get; private set; }
        public ushort Height { get; private set; }
        public ushort Weight { get; private set; }
    }
}