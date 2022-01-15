using System.Collections.Generic;

namespace BeaterLibrary.Formats.Pokémon {
    internal class Pokédex {
        public byte baseHp { get; private set; }
        public byte baseAttack { get; private set; }
        public byte baseDefense { get; private set; }
        public byte baseSpeed { get; private set; }
        public byte baseSpecialAttack { get; private set; }
        public byte baseSpecialDefense { get; private set; }
        public byte primaryType { get; private set; }
        public byte secondaryType { get; private set; }
        public byte catchRate { get; private set; }
        public byte stage { get; private set; }
        public ushort eVs { get; private set; }
        public List<ushort> items { get; private set; }
        public byte gender { get; private set; }
        public byte hatchCycle { get; private set; }
        public byte baseHappiness { get; private set; }
        public byte experienceRate { get; private set; }
        public byte eggGroup { get; private set; }
        public byte eggGroup2 { get; private set; }
        public byte primaryAbility { get; private set; }
        public byte secondaryAbility { get; private set; }
        public byte tertiaryAbility { get; private set; }
        public byte canFlee { get; private set; }
        public ushort formId { get; private set; }
        public ushort form { get; private set; }
        public byte numberOfForms { get; private set; }
        public byte color { get; private set; }
        public ushort baseExperience { get; private set; }
        public ushort height { get; private set; }
        public ushort weight { get; private set; }
    }
}