using System;
using System.Collections.Generic;
using System.IO;
using BeaterLibrary.Parsing;

namespace BeaterLibrary.Formats.Trainer {
    public class TrainerData {
        public TrainerData() {
            setPkmnHeldItem = false;
            setPkmnMoves = false;
            battleType = 0;
            numberOfPokemon = 0;
            items = new List<ushort>();
            ai = 0;
            isHealer = false;
            money = 0;
            prize = 0;
        }

        public TrainerData(byte[] data) : this() {
            var binary = new BinaryReaderEx(data);
            var format = binary.ReadByte();
            setPkmnHeldItem = ((format >> 0x1) & 0x1) == 1;
            setPkmnMoves = (format & 0x1) == 1;

            trainerClass = binary.ReadByte();
            battleType = binary.ReadByte();
            numberOfPokemon = binary.ReadByte();

            for (var i = 0; i < 4; ++i) items.Add(binary.ReadUInt16());

            ai = binary.ReadUInt32();

            if (numberOfPokemon > 0) {
                isHealer = binary.ReadByte() != 0x0;
                money = binary.ReadByte();
                prize = binary.ReadUInt16();
            }
        }

        public bool setPkmnHeldItem { get; set; }
        public bool setPkmnMoves { get; set; }
        public byte trainerClass { get; set; }
        public byte battleType { get; set; }
        public byte numberOfPokemon { get; set; }
        public List<ushort> items { get; set; }
        public uint ai { get; set; }
        public bool isHealer { get; set; }
        public byte money { get; set; }
        public ushort prize { get; set; }

        public void serialize(List<TrainerPokémonEntry> pkmn, string output) {
            var binary = new BinaryWriter(File.OpenWrite(output));
            binary.Write((byte) (((Convert.ToInt32(setPkmnHeldItem) << 0x1) & 0x1) |
                                 (Convert.ToInt32(setPkmnMoves) & 0x1)));
            binary.Write(trainerClass);
            binary.Write(battleType);
            binary.Write((byte) pkmn.Count);
            foreach (var t in items) binary.Write(t);
            binary.Write(ai);
            if (pkmn.Count > 0) {
                binary.Write(isHealer);
                binary.Write(money);
                binary.Write(prize);
            }

            binary.Close();
        }
    }
}