using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeaterLibrary.Formats.Trainer
{
    public class TrainerData
    {
        public bool HasItem { get; set; }
        public bool HasMoves { get; set; }
        public byte Class { get; set; }
        public byte BattleType { get; set; }
        public byte NumberOfPokemon { get; set; }
        public List<ushort> Items { get; set; }
        public uint AI { get; set; }
        public bool IsHealer { get; set; }
        public byte Money { get; set; }
        public ushort Prize { get; set; }

        public TrainerData() 
        {
            HasItem = false;
            HasMoves = false;
            BattleType = 0;
            NumberOfPokemon = 0;
            Items = new List<ushort>();
            AI = 0;
            IsHealer = false;
            Money = 0; 
            Prize = 0; 
        }
        
        public TrainerData(BinaryReader Binary) : this()
        {
            var Format = Binary.ReadByte();
            HasItem = ((Format >> 0x1) & 0x1) == 1;
            HasMoves = (Format & 0x1) == 1;

            Class = Binary.ReadByte();
            BattleType = Binary.ReadByte();
            NumberOfPokemon = Binary.ReadByte();
            
            if (HasItem)
                for (int i = 0; i < 4; ++i)
                    Items.Add(Binary.ReadUInt16());

            AI = Binary.ReadUInt32();
            IsHealer = Binary.ReadByte() != 0x0;
            Money = Binary.ReadByte();
            Prize = Binary.ReadUInt16();
        }
    }
}