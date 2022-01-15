﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeaterLibrary.Parsing;

namespace BeaterLibrary.Formats.Trainer {
    public class TrainerPokémonEntry {
        public TrainerPokémonEntry(bool pkmnSetMoves, bool pkmnSetHeldItem) {
            moves = new List<ushort> {0, 0, 0, 0};
            iv = 0;
            pid = 0;
            level = 0;
            species = 0;
            form = 0;
            heldItem = 0;
        }

        public TrainerPokémonEntry(byte[] data, bool pkmnSetMoves, bool pkmnSetHeldItem) {
            var binary = new BinaryReaderEx(data);
            moves = new List<ushort> {0, 0, 0, 0};
            heldItem = 0;
            iv = binary.ReadByte();
            pid = binary.ReadByte();
            ability = pid >> 4;
            gender = pid & 3;
            uBit = (pid >> 3) & 1;
            level = binary.ReadUInt16();
            species = binary.ReadUInt16();
            form = binary.ReadUInt16();
            if (pkmnSetHeldItem) 
                heldItem = binary.ReadUInt16();
            if (pkmnSetMoves)
                for (var i = 0; i < 4; ++i)
                    moves.Add(binary.ReadUInt16());
        }

        public byte iv { get; set; }
        public byte pid { get; set; }
        public ushort level { get; set; }
        public ushort species { get; set; }
        public ushort form { get; set; }
        public ushort heldItem { get; set; }
        public List<ushort> moves { get; set; }
        public int ability { get; set; }
        public int gender { get; set; }
        public int uBit { get; set; }

        public void serialize(bool pkmnSetMoves, bool pkmnSetHeldItem, BinaryWriter binary) {
            binary.Write(iv);
            binary.Write((byte) (((ability & 0xF) << 4) | ((uBit & 1) << 3) | (gender & 0x7)));
            binary.Write(level);
            binary.Write(species);
            binary.Write(form);
            if (pkmnSetHeldItem) binary.Write(heldItem);
            if (pkmnSetMoves)
                foreach (var m in moves)
                    binary.Write(m);
        }

        public override string ToString() {
            return $"Pokémon #{species}\nLevel: {level}\nForm: {form}\n";
        }
    }

    public class TrainerPokémonEntries {
        public TrainerPokémonEntries(byte[] data, bool pkmnSetMoves, int numberOfPokemon, bool pkmnSetHeldItem) {
            pokémonEntries = new List<TrainerPokémonEntry>();
            for (int i = 0, dataSize = 0x8 + (pkmnSetMoves ? 0x8 : 0x0) + (pkmnSetHeldItem ? 0x2 : 0x0); i < numberOfPokemon; ++i) {
                pokémonEntries.Add(new TrainerPokémonEntry(data.Skip(dataSize * i).Take(dataSize).ToArray(), pkmnSetMoves,
                    pkmnSetHeldItem));
            }
        }

        public List<TrainerPokémonEntry> pokémonEntries { get; }

        public void addPokémonEntry(TrainerPokémonEntry entry) {
            if (pokémonEntries.Count != 6)
                pokémonEntries.Add(entry);
            else
                throw new Exception("You can only have 6 Pokémon in your party.");
        }

        public void serialize(bool pkmnSetMoves, bool pkmnSetHeldItem, string output) {
            var binary = new BinaryWriter(File.OpenWrite(output));
            pokémonEntries.ForEach(entry => entry.serialize(pkmnSetMoves, pkmnSetHeldItem, binary));
            binary.Close();
        }

        public static void serialize(List<TrainerPokémonEntry> pokémonEntriesList, bool pkmnSetMoves,
            bool pkmnSetHeldItem, string output) {
            var binary = new BinaryWriter(File.OpenWrite(output));
            pokémonEntriesList.ForEach(entry => entry.serialize(pkmnSetMoves, pkmnSetHeldItem, binary));
            binary.Close();
        }
    }
}