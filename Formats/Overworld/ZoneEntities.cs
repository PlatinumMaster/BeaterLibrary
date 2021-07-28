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
    public class ZoneEntities
    {
        public List<NPC> NPCs { get; set; }
        public List<Trigger> Triggers { get; set; }
        public List<Interactable> Interactables { get; set; }
        public List<Warp> Warps { get; set; }
        public List<InitializationScript> LevelScripts { get; set; }

        public ZoneEntities()
        {
            NPCs = new List<NPC>();
            Triggers = new List<Trigger>();
            Interactables = new List<Interactable>();
            Warps = new List<Warp>();
            LevelScripts = new List<InitializationScript>();
        }

        public ZoneEntities(string overworld) : this()
        {
            // Initialize the overworld we will read from.
            BinaryReader b = new BinaryReader(File.Open(overworld, FileMode.Open));
            ParseOverworld(b);
            b.Close();
        }

        private void AddNew<T>(List<T> Target, T Entry) => Target.Add(Entry);

        private void RemoveSelected<T>(List<T> Target, int SelectedIndex)
        {
            if (SelectedIndex != -1)
                Target.RemoveAt(SelectedIndex);
            else
                throw new Exception($"You good? There's nothing to remove.");
        }

        public void AddNewInteractable(Interactable Entry) => AddNew(Interactables, Entry);
        public void AddNewNPC(NPC Entry) => AddNew(NPCs, Entry);
        public void AddNewWarp(Warp Entry) => AddNew(Warps, Entry);
        public void AddNewTrigger(Trigger Entry) => AddNew(Triggers, Entry);
        public void AddNewInitScript(InitializationScript Entry) => AddNew(LevelScripts, Entry);

        public void RemoveSelectedInteractable(int SelectedIndex) => RemoveSelected(Interactables, SelectedIndex);
        public void RemoveSelectedNPC(int SelectedIndex) => RemoveSelected(NPCs, SelectedIndex);
        public void RemoveSelectedWarp(int SelectedIndex) => RemoveSelected(Warps, SelectedIndex);
        public void RemoveSelectedTrigger(int SelectedIndex) => RemoveSelected(Triggers, SelectedIndex);
        public void RemoveSelectedInitScript(int SelectedIndex) => RemoveSelected(LevelScripts, SelectedIndex);

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
                LevelScripts.Add(new InitializationScript(b));

            b.BaseStream.Position += 0x2;

            int index = 0;
            while (b.PeekChar() != 0 && b.PeekChar() != -1 && index < LevelScripts.Count)
                LevelScripts[index++].SecondaryData = new InitializationScriptSecondaryData(b);
        }

        public void Serialize(string path)
        {
            BinaryWriter Binary = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
            Binary.Write((uint) (Interactable.Size * Interactables.Count + NPC.Size * NPCs.Count +
                                 Warp.Size * Warps.Count + Trigger.Size * Triggers.Count + 0x4));
            Binary.Write((byte) Interactables.Count);
            Binary.Write((byte) NPCs.Count);
            Binary.Write((byte) Warps.Count);
            Binary.Write((byte) Triggers.Count);

            // Because BinaryFormatter is "unsafe"...
            Interactables.ForEach(x =>
            {
                Binary.Write(x.Script);
                Binary.Write(x.Condition);
                Binary.Write(x.Interactibility);
                Binary.Write(x.RailIndex);
                Binary.Write(x.X);
                Binary.Write(x.Y);
                Binary.Write(x.Z * 0x10);
            });

            NPCs.ForEach(x =>
            {
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

            Warps.ForEach(x =>
            {
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

            Triggers.ForEach(x =>
            {
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

            LevelScripts.ForEach(x =>
            {
                Binary.Write(x.Unknown);
                Binary.Write(x.Unknown2);
                Binary.Write(x.Unknown3);
                SectionOffset += 0x6;
            });

            while (SectionOffset % 4 != 0)
            {
                Binary.Write((ushort) 0x0);
                SectionOffset += 0x2;
            }

            LevelScripts.ForEach(x =>
            {
                if (x.SecondaryData != null)
                {
                    Binary.Write(x.SecondaryData.Unknown);
                    Binary.Write(x.SecondaryData.Unknown2);
                    Binary.Write(x.SecondaryData.Unknown3);
                    SectionOffset += 0x6;
                }
            });

            while (SectionOffset % 4 != 0)
            {
                Binary.Write((ushort) 0x0);
                SectionOffset += 0x2;
            }

            Binary.Close();
        }
    }
}