using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Pokémon {
    public class WildEncounters {
        public WildEncounters() {
            GrassEntries = new List<WildEncounterEntry>();
            GrassDoubleEntries = new List<WildEncounterEntry>();
            GrassSpecialEntries = new List<WildEncounterEntry>();
            SurfEntries = new List<WildEncounterEntry>();
            SurfSpecialEntries = new List<WildEncounterEntry>();
            FishEntries = new List<WildEncounterEntry>();
            FishSpecialEntries = new List<WildEncounterEntry>();
            Regular = 0;
            DoubleBattle = 0;
            Special = 0;
            Surf = 0;
            SurfSpecial = 0;
            Fish = 0;
            FishSpecial = 0;
            ItemBattle = 0;
        }

        public WildEncounters(byte[] data) : this() {
            BinaryReader Binary = new BinaryReader(new MemoryStream(data));
            var Entries = new List<WildEncounterEntry>();
            Regular = Binary.ReadByte();
            DoubleBattle = Binary.ReadByte();
            Special = Binary.ReadByte();
            Surf = Binary.ReadByte();
            SurfSpecial = Binary.ReadByte();
            Fish = Binary.ReadByte();
            FishSpecial = Binary.ReadByte();
            ItemBattle = Binary.ReadByte();

            while (Binary.BaseStream.Position % 0xE8 != 0)
                Entries.Add(new WildEncounterEntry(Binary));

            GrassEntries = Entries.GetRange(0x0, 0xC);
            GrassDoubleEntries = Entries.GetRange(0xC, 0xC);
            GrassSpecialEntries = Entries.GetRange(0x18, 0xC);
            SurfEntries = Entries.GetRange(0x24, 0x5);
            SurfSpecialEntries = Entries.GetRange(0x29, 0x5);
            FishEntries = Entries.GetRange(0x2E, 0x5);
            FishSpecialEntries = Entries.GetRange(0x33, 0x5);
        }

        public List<WildEncounterEntry> GrassEntries { get; set; }
        public List<WildEncounterEntry> GrassDoubleEntries { get; set; }
        public List<WildEncounterEntry> GrassSpecialEntries { get; set; }
        public List<WildEncounterEntry> SurfEntries { get; set; }
        public List<WildEncounterEntry> SurfSpecialEntries { get; set; }
        public List<WildEncounterEntry> FishEntries { get; set; }
        public List<WildEncounterEntry> FishSpecialEntries { get; set; }
        public byte Regular { get; }
        public byte DoubleBattle { get; }
        public byte Special { get; }
        public byte Surf { get; }
        public byte SurfSpecial { get; }
        public byte Fish { get; }
        public byte FishSpecial { get; }
        public byte ItemBattle { get; }

        public byte[] Serialize() {
            MemoryStream Ms = new MemoryStream();
            var Binary = new BinaryWriter(Ms);
            Binary.Write((byte) GrassEntries.Count);
            Binary.Write((byte) GrassDoubleEntries.Count);
            Binary.Write((byte) GrassSpecialEntries.Count);
            Binary.Write((byte) SurfEntries.Count);
            Binary.Write((byte) SurfSpecialEntries.Count);
            Binary.Write((byte) FishEntries.Count);
            Binary.Write((byte) FishSpecialEntries.Count);
            Binary.Write(ItemBattle);

            // Grass entries
            foreach (var Entry in GrassEntries)
                Entry.Serialize(Binary);
            for (var I = GrassEntries.Count; I < 0xC; ++I)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in GrassDoubleEntries)
                Entry.Serialize(Binary);
            for (var I = GrassDoubleEntries.Count; I < 0xC; ++I)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in GrassSpecialEntries)
                Entry.Serialize(Binary);
            for (var I = GrassSpecialEntries.Count; I < 0xC; ++I)
                new WildEncounterEntry().Serialize(Binary);

            // Surf Entries
            foreach (var Entry in SurfEntries)
                Entry.Serialize(Binary);
            for (var I = SurfEntries.Count; I < 0x5; ++I)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in SurfSpecialEntries)
                Entry.Serialize(Binary);
            for (var I = SurfSpecialEntries.Count; I < 0x5; ++I)
                new WildEncounterEntry().Serialize(Binary);

            // Fishing Entries
            foreach (var Entry in FishEntries)
                Entry.Serialize(Binary);
            for (var I = FishEntries.Count; I < 0x5; ++I)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in FishSpecialEntries)
                Entry.Serialize(Binary);
            for (var I = FishSpecialEntries.Count; I < 0x5; ++I)
                new WildEncounterEntry().Serialize(Binary);
            Binary.Close();
            return Ms.ToArray();
        }
    }

    public class WildEncounterEntry {
        public WildEncounterEntry() {
            NationalDexNumber = 0;
            FormNumber = 0;
            MinimumLevel = 0;
            MaximumLevel = 0;
        }

        public WildEncounterEntry(BinaryReader binary) {
            var DexAndForm = binary.ReadUInt16();
            NationalDexNumber = (ushort) (DexAndForm & 0x7FF);
            FormNumber = (ushort) (DexAndForm >> 0xB);
            MinimumLevel = binary.ReadByte();
            MaximumLevel = binary.ReadByte();
        }

        public ushort NationalDexNumber { get; set; }
        public ushort FormNumber { get; set; }
        public byte MinimumLevel { get; set; }
        public byte MaximumLevel { get; set; }

        public void Serialize(BinaryWriter binary) {
            binary.Write((ushort) ((ushort) (FormNumber << 0xB) | NationalDexNumber));
            binary.Write(MinimumLevel);
            binary.Write(MaximumLevel);
        }

        public override string ToString() {
            return $"Pokémon: {NationalDexNumber}\nForm: {FormNumber}\nLevel Range: ({MinimumLevel} - {MaximumLevel})";
        }
    }
}