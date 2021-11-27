using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Trainer
{
    public class TrainerData
    {
        public TrainerData()
        {
            HasItems = false;
            OverrideMoves = false;
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
            HasItems = ((Format >> 0x1) & 0x1) == 1;
            OverrideMoves = (Format & 0x1) == 1;

            Class = Binary.ReadByte();
            BattleType = Binary.ReadByte();
            NumberOfPokemon = Binary.ReadByte();

            for (var i = 0; i < 4; ++i)
                Items.Add(Binary.ReadUInt16());

            AI = Binary.ReadUInt32();

            if (NumberOfPokemon > 0)
            {
                IsHealer = Binary.ReadByte() != 0x0;
                Money = Binary.ReadByte();
                Prize = Binary.ReadUInt16();
            }
        }

        public bool HasItems { get; set; }
        public bool OverrideMoves { get; set; }
        public byte Class { get; set; }
        public byte BattleType { get; set; }
        public byte NumberOfPokemon { get; set; }
        public List<ushort> Items { get; set; }
        public uint AI { get; set; }
        public bool IsHealer { get; set; }
        public byte Money { get; set; }
        public ushort Prize { get; set; }

        public void Serialize(List<TrainerPokémonEntry> Pkmn, string Output)
        {
            var Binary = new BinaryWriter(File.OpenWrite(Output));
            Binary.Write((byte) (((Convert.ToInt32(HasItems) << 0x1) & 0x1) | (Convert.ToInt32(OverrideMoves) & 0x1)));
            Binary.Write(Class);
            Binary.Write(BattleType);
            Binary.Write((byte) Pkmn.Count);
            for (var i = 0; i < Items.Count; ++i)
                Binary.Write(Items[i]);
            Binary.Write(AI);
            if (Pkmn.Count > 0)
            {
                Binary.Write(IsHealer);
                Binary.Write(Money);
                Binary.Write(Prize);
            }

            Binary.Close();
        }
    }
}