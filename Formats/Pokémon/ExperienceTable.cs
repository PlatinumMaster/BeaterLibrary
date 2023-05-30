using System.Collections.Generic;

namespace BeaterLibrary.Formats.Pokémon {
    internal class ExperienceTable {
        public Dictionary<byte, uint> ExperienceTableMap;
        public ExperienceTable(Dictionary<byte, uint> experienceTableMap) {
            this.ExperienceTableMap = experienceTableMap;
        }
    }
}