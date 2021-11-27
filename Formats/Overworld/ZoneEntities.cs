using System;
using System.Collections.Generic;
using System.IO;
using BeaterLibrary.Formats.Scripts;

namespace BeaterLibrary.Formats.Overworld
{
    public class ZoneEntities
    {
        public ZoneEntities()
        {
            NPCs = new List<NPC>();
            Triggers = new List<Trigger>();
            Interactables = new List<Interactable>();
            Warps = new List<Warp>();
            InitializationScripts = new List<InitializationScript>();
            TriggerRelatedEntries = new List<TriggerRelated>();
        }

        public ZoneEntities(string overworld) : this()
        {
            // Initialize the overworld we will read from.
            var b = new BinaryReader(File.Open(overworld, FileMode.Open));
            ParseOverworld(b);
            b.Close();
        }

        public ZoneEntities(byte[] Data) : this()
        {
            // Initialize the overworld we will read from.
            var b = new BinaryReader(new MemoryStream(Data));
            ParseOverworld(b);
            b.Close();
        }

        public List<NPC> NPCs { get; set; }
        public List<Trigger> Triggers { get; set; }
        public List<Interactable> Interactables { get; set; }
        public List<Warp> Warps { get; set; }
        public List<InitializationScript> InitializationScripts { get; set; }
        public List<TriggerRelated> TriggerRelatedEntries { get; set; }

        public static void Serialize(List<Interactable> Interactables, List<NPC> NPCs, List<Warp> Warps,
            List<Trigger> Triggers, List<InitializationScript> InitializationScripts, List<TriggerRelated> Trigger2,
            string Output)
        {
            new ZoneEntities
            {
                Interactables = Interactables,
                NPCs = NPCs,
                Warps = Warps,
                Triggers = Triggers,
                TriggerRelatedEntries = Trigger2,
                InitializationScripts = InitializationScripts
            }.Serialize(Output);
        }

        private void AddNew<T>(List<T> Target, T Entry)
        {
            Target.Add(Entry);
        }

        private void RemoveSelected<T>(List<T> Target, int SelectedIndex)
        {
            if (SelectedIndex != -1)
                Target.RemoveAt(SelectedIndex);
            else
                throw new Exception("You good? There's nothing to remove.");
        }

        public void AddNewInteractable(Interactable Entry)
        {
            AddNew(Interactables, Entry);
        }

        public void AddNewNPC(NPC Entry)
        {
            AddNew(NPCs, Entry);
        }

        public void AddNewWarp(Warp Entry)
        {
            AddNew(Warps, Entry);
        }

        public void AddNewTrigger(Trigger Entry)
        {
            AddNew(Triggers, Entry);
        }

        public void AddNewInitScript(InitializationScript Entry)
        {
            AddNew(InitializationScripts, Entry);
        }

        public void RemoveSelectedInteractable(int SelectedIndex)
        {
            RemoveSelected(Interactables, SelectedIndex);
        }

        public void RemoveSelectedNPC(int SelectedIndex)
        {
            RemoveSelected(NPCs, SelectedIndex);
        }

        public void RemoveSelectedWarp(int SelectedIndex)
        {
            RemoveSelected(Warps, SelectedIndex);
        }

        public void RemoveSelectedTrigger(int SelectedIndex)
        {
            RemoveSelected(Triggers, SelectedIndex);
        }

        public void RemoveSelectedInitScript(int SelectedIndex)
        {
            RemoveSelected(InitializationScripts, SelectedIndex);
        }

        public void ParseOverworld(BinaryReader Binary)
        {
            var fileSize = Binary.ReadUInt32();
            var interactableCount = Binary.ReadByte();
            var npcCount = Binary.ReadByte();
            var warpCount = Binary.ReadByte();
            var triggerCount = Binary.ReadByte();

            for (var i = 0; i < interactableCount; ++i)
                Interactables.Add(new Interactable(Binary));

            for (var i = 0; i < npcCount; ++i)
                NPCs.Add(new NPC(Binary));

            for (var i = 0; i < warpCount; ++i)
                Warps.Add(new Warp(Binary));

            for (var i = 0; i < triggerCount; ++i)
                Triggers.Add(new Trigger(Binary));

            while ((Binary.PeekChar() & 0x7FFF) != 0)
                InitializationScripts.Add(new InitializationScript(Binary));

            Binary.BaseStream.Seek(0x2, SeekOrigin.Current);

            while ((Binary.PeekChar() & 0x7FFF) != 0)
                TriggerRelatedEntries.Add(new TriggerRelated(Binary));
        }

        public void Serialize(string path)
        {
            var Binary = new BinaryWriter(File.OpenWrite(path));
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
                Binary.Write(x.Z);
                Binary.Write(x.Unknown3);
                Binary.Write(x.Y);
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

            var SectionOffset = 0;

            InitializationScripts.ForEach(x =>
            {
                Binary.Write(x.Type);
                Binary.Write(x.ScriptIndex);
                Binary.Write(x.Unknown);
                SectionOffset += 0x6;
            });
            
            Binary.Write((ushort) 0x0);

            TriggerRelatedEntries.ForEach(x =>
            {
                Binary.Write(x.Variable);
                Binary.Write(x.Value);
                Binary.Write(x.ScriptID);
            });
            
            Binary.Write((ushort) 0x0);

            Binary.Close();
        }
    }
}