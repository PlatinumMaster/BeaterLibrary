using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Trainer
{
    public class TrainerPokémonEntry
    {
        public TrainerPokémonEntry(bool HasMoves, bool HasItem)
        {
            Moves = new List<ushort> {0, 0, 0, 0};
            IV = 0;
            PID = 0;
            Level = 0;
            Species = 0;
            Form = 0;
            HeldItem = 0;
        }

        public TrainerPokémonEntry(BinaryReader Binary, bool HasMoves, bool HasItem)
        {
            Moves = new List<ushort> {0, 0, 0, 0};
            IV = Binary.ReadByte();
            PID = Binary.ReadByte();
            Ability = PID >> 4;
            Gender = PID & 3;
            uBit = (PID >> 3) & 1;
            Level = Binary.ReadUInt16();
            Species = Binary.ReadUInt16();
            Form = Binary.ReadUInt16();
            if (HasItem)
                HeldItem = Binary.ReadUInt16();
            if (HasMoves)
                for (var i = 0; i < 4; ++i)
                    Moves.Add(Binary.ReadUInt16());
        }

        public byte IV { get; set; }
        public byte PID { get; set; }
        public ushort Level { get; set; }
        public ushort Species { get; set; }
        public ushort Form { get; set; }
        public ushort HeldItem { get; set; }
        public List<ushort> Moves { get; set; }
        public int Ability { get; set; }
        public int Gender { get; set; }
        public int uBit { get; set; }

        public void Serialize(bool HasMoves, bool HasItem, BinaryWriter Binary)
        {
            Binary.Write(IV);
            Binary.Write((byte) (((Ability & 0xF) << 4) | ((uBit & 1) << 3) | (Gender & 0x7)));
            Binary.Write(Level);
            Binary.Write(Species);
            Binary.Write(Form);
            if (HasItem)
                Binary.Write(HeldItem);
            if (HasMoves)
                for (var i = 0; i < 4; ++i)
                    Binary.Write(Moves[i]);
        }

        public override string ToString()
        {
            return $"Pokémon #{Species}\nLevel: {Level}\nForm: {Form}\n";
        }
    }

    public class TrainerPokémonEntries
    {
        public TrainerPokémonEntries(BinaryReader Binary, bool HasMoves, int NumberOfPokemon, bool HasItem)
        {
            PokémonEntries = new List<TrainerPokémonEntry>();
            for (var i = 0; i < NumberOfPokemon; ++i)
                PokémonEntries.Add(new TrainerPokémonEntry(Binary, HasMoves, HasItem));
        }

        public List<TrainerPokémonEntry> PokémonEntries { get; }

        public void AddPokémonEntry(TrainerPokémonEntry Entry)
        {
            if (PokémonEntries.Count != 6)
                PokémonEntries.Add(Entry);
            else
                throw new Exception("Lmao you ain't shit Ghetsis bruh nice try");
        }

        public void Serialize(bool HasMoves, bool HasItem, string Output)
        {
            var Binary = new BinaryWriter(File.OpenWrite(Output));
            PokémonEntries.ForEach(Entry => Entry.Serialize(HasMoves, HasItem, Binary));
            Binary.Close();
        }

        public static void Serialize(List<TrainerPokémonEntry> PokémonEntriesList, bool HasMoves, bool HasItem,
            string Output)
        {
            var Binary = new BinaryWriter(File.OpenWrite(Output));
            PokémonEntriesList.ForEach(Entry => Entry.Serialize(HasMoves, HasItem, Binary));
            Binary.Close();
        }
    }
}