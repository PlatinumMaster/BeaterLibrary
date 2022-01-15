using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Pokémon {
    public class WildEncounters {
        public WildEncounters() {
            grassEntries = new List<WildEncounterEntry>();
            grassDoubleEntries = new List<WildEncounterEntry>();
            grassSpecialEntries = new List<WildEncounterEntry>();
            surfEntries = new List<WildEncounterEntry>();
            surfSpecialEntries = new List<WildEncounterEntry>();
            fishEntries = new List<WildEncounterEntry>();
            fishSpecialEntries = new List<WildEncounterEntry>();
            regular = 0;
            doubleBattle = 0;
            special = 0;
            surf = 0;
            surfSpecial = 0;
            fish = 0;
            fishSpecial = 0;
            itemBattle = 0;
        }

        public WildEncounters(byte[] data) : this() {
            BinaryReader binary = new BinaryReader(new MemoryStream(data));
            var entries = new List<WildEncounterEntry>();
            regular = binary.ReadByte();
            doubleBattle = binary.ReadByte();
            special = binary.ReadByte();
            surf = binary.ReadByte();
            surfSpecial = binary.ReadByte();
            fish = binary.ReadByte();
            fishSpecial = binary.ReadByte();
            itemBattle = binary.ReadByte();

            while (binary.BaseStream.Position % 0xE8 != 0)
                entries.Add(new WildEncounterEntry(binary));

            grassEntries = entries.GetRange(0x0, 0xC);
            grassDoubleEntries = entries.GetRange(0xC, 0xC);
            grassSpecialEntries = entries.GetRange(0x18, 0xC);
            surfEntries = entries.GetRange(0x24, 0x5);
            surfSpecialEntries = entries.GetRange(0x29, 0x5);
            fishEntries = entries.GetRange(0x2E, 0x5);
            fishSpecialEntries = entries.GetRange(0x33, 0x5);
        }

        public List<WildEncounterEntry> grassEntries { get; set; }
        public List<WildEncounterEntry> grassDoubleEntries { get; set; }
        public List<WildEncounterEntry> grassSpecialEntries { get; set; }
        public List<WildEncounterEntry> surfEntries { get; set; }
        public List<WildEncounterEntry> surfSpecialEntries { get; set; }
        public List<WildEncounterEntry> fishEntries { get; set; }
        public List<WildEncounterEntry> fishSpecialEntries { get; set; }
        public byte regular { get; }
        public byte doubleBattle { get; }
        public byte special { get; }
        public byte surf { get; }
        public byte surfSpecial { get; }
        public byte fish { get; }
        public byte fishSpecial { get; }
        public byte itemBattle { get; }

        public byte[] serialize() {
            MemoryStream ms = new MemoryStream();
            var binary = new BinaryWriter(ms);
            binary.Write((byte) grassEntries.Count);
            binary.Write((byte) grassDoubleEntries.Count);
            binary.Write((byte) grassSpecialEntries.Count);
            binary.Write((byte) surfEntries.Count);
            binary.Write((byte) surfSpecialEntries.Count);
            binary.Write((byte) fishEntries.Count);
            binary.Write((byte) fishSpecialEntries.Count);
            binary.Write(itemBattle);

            // Grass entries
            foreach (var entry in grassEntries)
                entry.serialize(binary);
            for (var i = grassEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().serialize(binary);

            foreach (var entry in grassDoubleEntries)
                entry.serialize(binary);
            for (var i = grassDoubleEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().serialize(binary);

            foreach (var entry in grassSpecialEntries)
                entry.serialize(binary);
            for (var i = grassSpecialEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().serialize(binary);

            // Surf Entries
            foreach (var entry in surfEntries)
                entry.serialize(binary);
            for (var i = surfEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().serialize(binary);

            foreach (var entry in surfSpecialEntries)
                entry.serialize(binary);
            for (var i = surfSpecialEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().serialize(binary);

            // Fishing Entries
            foreach (var entry in fishEntries)
                entry.serialize(binary);
            for (var i = fishEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().serialize(binary);

            foreach (var entry in fishSpecialEntries)
                entry.serialize(binary);
            for (var i = fishSpecialEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().serialize(binary);
            binary.Close();
            return ms.ToArray();
        }
    }

    public class WildEncounterEntry {
        public WildEncounterEntry() {
            nationalDexNumber = 0;
            formNumber = 0;
            minimumLevel = 0;
            maximumLevel = 0;
        }

        public WildEncounterEntry(BinaryReader binary) {
            var dexAndForm = binary.ReadUInt16();
            nationalDexNumber = (ushort) (dexAndForm & 0x7FF);
            formNumber = (ushort) (dexAndForm >> 0xB);
            minimumLevel = binary.ReadByte();
            maximumLevel = binary.ReadByte();
        }

        public ushort nationalDexNumber { get; set; }
        public ushort formNumber { get; set; }
        public byte minimumLevel { get; set; }
        public byte maximumLevel { get; set; }

        public void serialize(BinaryWriter binary) {
            binary.Write((ushort) ((ushort) (formNumber << 0xB) | nationalDexNumber));
            binary.Write(minimumLevel);
            binary.Write(maximumLevel);
        }

        public override string ToString() {
            return $"Pokémon: {nationalDexNumber}\nForm: {formNumber}\nLevel Range: ({minimumLevel} - {maximumLevel})";
        }
    }
}