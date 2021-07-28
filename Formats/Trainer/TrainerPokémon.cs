using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeaterLibrary.Formats.Trainer
{
    public class TrainerPokémonEntry
    {
        public byte IV {get; set;}
        public byte PID {get; set;}
        public ushort Level {get; set;}
        public ushort Species {get; set;}
        public ushort Form {get; set;}
        public ushort HeldItem {get; set;}
        public List<ushort> Moves {get; set;}
        
        public TrainerPokémonEntry(BinaryReader Binary, bool HasMoves, bool HasItem)
        {
            Moves = new List<ushort>();
            IV = Binary.ReadByte();
            PID = Binary.ReadByte();
            Level = Binary.ReadUInt16();
            Species = Binary.ReadUInt16();
            Form = Binary.ReadUInt16();
            if (HasItem)
                HeldItem = Binary.ReadUInt16();
            if (HasMoves)
                for (int i = 0; i < 4; ++i) 
                    Moves.Add(Binary.ReadUInt16());
        }
    }
    public class TrainerPokémonEntries
    {
        public List<TrainerPokémonEntry> PokémonEntries { get; }

        public void AddPokémonEntry(TrainerPokémonEntry Entry)
        {
            if (PokémonEntries.Count != 6)
                PokémonEntries.Add(Entry);
            else
                throw new Exception("Lmao you ain't shit Ghetsis bruh nice try");
        }

        public TrainerPokémonEntries(BinaryReader Binary, bool HasItem, bool HasMoves, int NumberOfPokemon)
        {
            PokémonEntries = new List<TrainerPokémonEntry>();
            for (int i = 0; i < NumberOfPokemon; ++i)
                PokémonEntries.Add(new TrainerPokémonEntry(Binary, HasMoves, HasItem));
        }
    }
}