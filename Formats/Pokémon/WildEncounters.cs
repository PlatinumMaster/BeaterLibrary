using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Pokémon
{
    public class WildEncounters
    {
        public WildEncounters()
        {
            GrassEntries = new List<WildEncounterEntry>();
            GrassDoubleEntries = new List<WildEncounterEntry>();
            GrassShakeEntries = new List<WildEncounterEntry>();
            SurfEntries = new List<WildEncounterEntry>();
            SurfSpotEntries = new List<WildEncounterEntry>();
            FishEntries = new List<WildEncounterEntry>();
            FishSpotEntries = new List<WildEncounterEntry>();
            Regular = 0;
            DoubleBattle = 0;
            Shake = 0;
            Surf = 0;
            SurfRare = 0;
            Fish = 0;
            FishRare = 0;
            ItemBattle = 0;
        }

        public WildEncounters(BinaryReader Binary) : this()
        {
            var Entries = new List<WildEncounterEntry>();
            Regular = Binary.ReadByte();
            DoubleBattle = Binary.ReadByte();
            Shake = Binary.ReadByte();
            Surf = Binary.ReadByte();
            SurfRare = Binary.ReadByte();
            Fish = Binary.ReadByte();
            FishRare = Binary.ReadByte();
            ItemBattle = Binary.ReadByte();

            while (Binary.BaseStream.Position % 0xE8 != 0)
                Entries.Add(new WildEncounterEntry(Binary));

            GrassEntries = Entries.GetRange(0x0, 0xC);
            GrassDoubleEntries = Entries.GetRange(0xC, 0xC);
            GrassShakeEntries = Entries.GetRange(0x18, 0xC);
            SurfEntries = Entries.GetRange(0x24, 0x5);
            SurfSpotEntries = Entries.GetRange(0x29, 0x5);
            FishEntries = Entries.GetRange(0x2E, 0x5);
            FishSpotEntries = Entries.GetRange(0x33, 0x5);
        }

        public List<WildEncounterEntry> GrassEntries { get; set; }
        public List<WildEncounterEntry> GrassDoubleEntries { get; set; }
        public List<WildEncounterEntry> GrassShakeEntries { get; set; }
        public List<WildEncounterEntry> SurfEntries { get; set; }
        public List<WildEncounterEntry> SurfSpotEntries { get; set; }
        public List<WildEncounterEntry> FishEntries { get; set; }
        public List<WildEncounterEntry> FishSpotEntries { get; set; }
        public byte Regular { get; }
        public byte DoubleBattle { get; }
        public byte Shake { get; }
        public byte Surf { get; }
        public byte SurfRare { get; }
        public byte Fish { get; }
        public byte FishRare { get; }
        public byte ItemBattle { get; }

        public void Serialize(string path)
        {
            var Binary = new BinaryWriter(File.OpenWrite(path));
            Binary.Write((byte) GrassEntries.Count);
            Binary.Write((byte) GrassDoubleEntries.Count);
            Binary.Write((byte) GrassShakeEntries.Count);
            Binary.Write((byte) SurfEntries.Count);
            Binary.Write((byte) SurfSpotEntries.Count);
            Binary.Write((byte) FishEntries.Count);
            Binary.Write((byte) FishSpotEntries.Count);
            Binary.Write(ItemBattle);

            // Grass entries
            foreach (var Entry in GrassEntries)
                Entry.Serialize(Binary);
            for (var i = GrassEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in GrassDoubleEntries)
                Entry.Serialize(Binary);
            for (var i = GrassDoubleEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in GrassShakeEntries)
                Entry.Serialize(Binary);
            for (var i = GrassShakeEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().Serialize(Binary);

            // Surf Entries
            foreach (var Entry in SurfEntries)
                Entry.Serialize(Binary);
            for (var i = SurfEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in SurfSpotEntries)
                Entry.Serialize(Binary);
            for (var i = SurfSpotEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);

            // Fishing Entries
            foreach (var Entry in FishEntries)
                Entry.Serialize(Binary);
            for (var i = FishEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (var Entry in FishSpotEntries)
                Entry.Serialize(Binary);
            for (var i = FishSpotEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);
            Binary.Close();
        }
    }

    public class WildEncounterEntry
    {
        public WildEncounterEntry()
        {
            NationalDexNumber = 0;
            FormNumber = 0;
            MinimumLevel = 0;
            MaximumLevel = 0;
        }

        public WildEncounterEntry(BinaryReader Binary)
        {
            var DexAndForm = Binary.ReadUInt16();
            NationalDexNumber = (ushort) (DexAndForm & 0x7FF);
            FormNumber = (ushort) (DexAndForm >> 0xB);
            MinimumLevel = Binary.ReadByte();
            MaximumLevel = Binary.ReadByte();
        }

        public ushort NationalDexNumber { get; set; }
        public ushort FormNumber { get; set; }
        public byte MinimumLevel { get; set; }
        public byte MaximumLevel { get; set; }

        public void Serialize(BinaryWriter Binary)
        {
            Binary.Write((ushort) ((ushort) (FormNumber << 0xB) + NationalDexNumber));
            Binary.Write(MinimumLevel);
            Binary.Write(MaximumLevel);
        }

        public override string ToString()
        {
            return $"Pokémon: {NationalDexNumber}\nForm: {FormNumber}\nLevel Range: ({MinimumLevel} - {MaximumLevel})";
        }
    }
}