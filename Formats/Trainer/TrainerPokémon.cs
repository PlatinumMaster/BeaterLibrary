using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Formats.Trainer
{
    struct TrainerPokemonEntry
    {
        byte Difficulty;
        byte Unknown;
        byte Level; 
        byte Unknown2;
        ushort Pokemon;
        byte Unknown3;
        byte Unknown4;
        ushort HeldItem;
        List<ushort> Moves;
    }
    class TrainerPokémon
    {
    }
}
