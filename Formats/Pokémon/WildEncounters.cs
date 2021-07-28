using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BeaterLibrary.Formats.Pokémon
{
    public class WildEncounters
    {
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
            List<WildEncounterEntry> Entries = new List<WildEncounterEntry>();
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
        public void Serialize(string path)
        {
            BinaryWriter Binary = new BinaryWriter(File.OpenWrite(path));
            Binary.Write((byte)GrassEntries.Count);
            Binary.Write((byte)GrassDoubleEntries.Count);
            Binary.Write((byte)GrassShakeEntries.Count);
            Binary.Write((byte)SurfEntries.Count);
            Binary.Write((byte)SurfSpotEntries.Count);
            Binary.Write((byte)FishEntries.Count);
            Binary.Write((byte)FishSpotEntries.Count);
            Binary.Write(ItemBattle);

            // Grass entries
            foreach (WildEncounterEntry Entry in GrassEntries)
                Entry.Serialize(Binary);
            for (int i = GrassEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (WildEncounterEntry Entry in GrassDoubleEntries)
                Entry.Serialize(Binary);
            for (int i = GrassDoubleEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (WildEncounterEntry Entry in GrassShakeEntries)
                Entry.Serialize(Binary);
            for (int i = GrassShakeEntries.Count; i < 0xC; ++i)
                new WildEncounterEntry().Serialize(Binary);

            // Surf Entries
            foreach (WildEncounterEntry Entry in SurfEntries)
                Entry.Serialize(Binary);
            for (int i = SurfEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (WildEncounterEntry Entry in SurfSpotEntries)
                Entry.Serialize(Binary);
            for (int i = SurfSpotEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);

            // Fishing Entries
            foreach (WildEncounterEntry Entry in FishEntries)
                Entry.Serialize(Binary);
            for (int i = FishEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);

            foreach (WildEncounterEntry Entry in FishSpotEntries)
                Entry.Serialize(Binary);
            for (int i = FishSpotEntries.Count; i < 0x5; ++i)
                new WildEncounterEntry().Serialize(Binary);
            Binary.Close();
        }
    }

    public class WildEncounterEntry
    {
        public ushort NationalDexNumber { get; set; }
        public ushort FormNumber { get; set; }
        public byte MinimumLevel { get; set; }
        public byte MaximumLevel { get; set; }

        public WildEncounterEntry()
        {
            NationalDexNumber = 0;
            FormNumber = 0;
            MinimumLevel = 0;
            MaximumLevel = 0;
        }

        public WildEncounterEntry(BinaryReader Binary)
        {
            ushort DexAndForm = Binary.ReadUInt16();
            NationalDexNumber = (ushort) (DexAndForm & 0x7FF);
            FormNumber = (ushort) (DexAndForm >> 0xB);
            MinimumLevel = Binary.ReadByte();
            MaximumLevel = Binary.ReadByte();
        }

        public void Serialize(BinaryWriter Binary)
        {
            Binary.Write((ushort) ((ushort) (FormNumber << 0xB) + NationalDexNumber));
            Binary.Write(MinimumLevel);
            Binary.Write(MaximumLevel);
        }

        public override string ToString() => $"Pokémon: {NationalDexNumber}\nForm: {FormNumber}\nLevel Range: ({MinimumLevel} - {MaximumLevel})";
    }
}