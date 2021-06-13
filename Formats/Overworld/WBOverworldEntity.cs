using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using BeaterLibrary.Formats.Scripts;

namespace BeaterLibrary.Formats.Furniture
{
    public class WBOverworldEntity
    {
        public List<NPC> NPCs { get; set; }
        public List<Trigger> Triggers { get; set; }
        public List<Interactable> Interactables { get; set; }
        public List<Warp> Warps { get; set; }
        public List<LevelScriptDeclaration> LevelScripts { get; set; }

        public WBOverworldEntity()
        {
            NPCs = new List<NPC>();
            Triggers = new List<Trigger>();
            Interactables = new List<Interactable>();
            Warps = new List<Warp>();
            LevelScripts = new List<LevelScriptDeclaration>();
        }
        public WBOverworldEntity(string overworld) : this()
        {
            // Initialize the overworld we will read from.
            BinaryReader b = new BinaryReader(File.Open(overworld, FileMode.Open));
            ParseOverworld(b);
            b.Close();
        }

        public void ParseOverworld(BinaryReader b)
        {
            uint fileSize = b.ReadUInt32();
            if (fileSize < 0x4)
            {
                b.Close();
                throw new Exception("Invalid overworld container.");
            }

            byte interactableCount = b.ReadByte();
            byte npcCount = b.ReadByte();
            byte warpCount = b.ReadByte();
            byte triggerCount = b.ReadByte();

            for (int i = 0; i < interactableCount; ++i)
                Interactables.Add(new Interactable(b));

            for (int i = 0; i < npcCount; ++i)
                NPCs.Add(new NPC(b));

            for (int i = 0; i < warpCount; ++i)
                Warps.Add(new Warp(b));

            for (int i = 0; i < triggerCount; ++i)
                Triggers.Add(new Trigger(b));
            
            while (b.PeekChar() != 0)
                LevelScripts.Add(new LevelScriptDeclaration(b));

            b.BaseStream.Position += 0x2;

            int index = 0;
            while (b.PeekChar() != 0 && b.PeekChar() != -1 && index < LevelScripts.Count)
                LevelScripts[index++].Data = new LevelScriptData(b);
        }

        public void Serialize(string path) {
            BinaryWriter Binary = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
            Binary.Write((uint)(Interactable.Size * Interactables.Count + NPC.Size * NPCs.Count + Warp.Size * Warps.Count + Trigger.Size * Triggers.Count + 0x4));
            Binary.Write((byte)Interactables.Count);
            Binary.Write((byte)NPCs.Count);
            Binary.Write((byte)Warps.Count);
            Binary.Write((byte)Triggers.Count);

            // Because BinaryFormatter is "unsafe"...
            Interactables.ForEach(x => {
                Binary.Write(x.Script);
                Binary.Write(x.Condition);
                Binary.Write(x.Interactibility);
                Binary.Write(x.RailIndex);
                Binary.Write(x.X);
                Binary.Write(x.Y);
                Binary.Write(x.Z * 0x10);
            });

            NPCs.ForEach(x => {
                Binary.Write(x.ID);
                Binary.Write(x.ModelID);
                Binary.Write(x.MovementPermission);
                Binary.Write(x.Type);
                Binary.Write(x.SpawnFlag);
                Binary.Write(x.ScriptID);
                Binary.Write(x.FaceDirection);
                Binary.Write(x.SightRange);
                Binary.Write(x.Unknown);
                Binary.Write(x.Unknown2);
                Binary.Write(x.TraversalWidth);
                Binary.Write(x.TraversalHeight);
                Binary.Write(x.StartingX);
                Binary.Write(x.StartingY);
                Binary.Write(x.X);
                Binary.Write(x.Y);
                Binary.Write(x.Unknown3);
                Binary.Write(x.Z);
            });

            Warps.ForEach(x => {
                Binary.Write(x.TargetZone);
                Binary.Write(x.TargetWarp);
                Binary.Write(x.ContactDirection);
                Binary.Write(x.TransitionType);
                Binary.Write(x.CoordinateType);
                Binary.Write(x.X);
                Binary.Write(x.Z);
                Binary.Write(x.Y);
                Binary.Write(x.W);
                Binary.Write(x.H);
                Binary.Write(x.Rail);
            });

            Triggers.ForEach(x => {
                Binary.Write(x.Script);
                Binary.Write(x.ValueNeededForExecution);
                Binary.Write(x.Variable);
                Binary.Write(x.Unknown);
                Binary.Write(x.Unknown2);
                Binary.Write(x.X);
                Binary.Write(x.Y);
                Binary.Write(x.Z);
                Binary.Write(x.W);
                Binary.Write(x.H);
                Binary.Write(x.Unknown3);
            });

            int SectionOffset = 0;

            LevelScripts.ForEach(x => {
                Binary.Write(x.Unknown);
                Binary.Write(x.Unknown2);
                Binary.Write(x.Unknown3);
                SectionOffset += 0x6;
            });

            while (SectionOffset % 4 != 0)
            {
                Binary.Write((ushort)0x0);
                SectionOffset += 0x2;
            }

            LevelScripts.ForEach(x => {
                if (x.Data != null)
                {
                    Binary.Write(x.Data.Unknown);
                    Binary.Write(x.Data.Unknown2);
                    Binary.Write(x.Data.Unknown3);
                    SectionOffset += 0x6;
                }
            });

            while (SectionOffset % 4 != 0)
            {
                Binary.Write((ushort)0x0);
                SectionOffset += 0x2;
            }

            Binary.Close();
        }
    }
}
