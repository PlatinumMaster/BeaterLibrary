using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Zone {
    public class ZoneEntities : FieldObject {
        public ZoneEntities() {
            npcs = new List<NPC>();
            triggers = new List<Trigger>();
            interactables = new List<Proxy>();
            warps = new List<Warp>();
            initializationScripts = new List<InitializationScript>();
            triggerRelatedEntries = new List<TriggerRelated>();
        }

        public ZoneEntities(string overworld) : this() {
            // Initialize the overworld we will read from.
            var b = new BinaryReader(File.Open(overworld, FileMode.Open));
            parseOverworld(b);
            b.Close();
        }

        public ZoneEntities(byte[] data) : this() {
            // Initialize the overworld we will read from.
            var b = new BinaryReader(new MemoryStream(data));
            parseOverworld(b);
            b.Close();
        }

        public List<NPC> npcs { get; set; }
        public List<Trigger> triggers { get; set; }
        public List<Proxy> interactables { get; set; }
        public List<Warp> warps { get; set; }
        public List<InitializationScript> initializationScripts { get; set; }
        public List<TriggerRelated> triggerRelatedEntries { get; set; }

        public static void serialize(List<Proxy> interactables, List<NPC> npCs, List<Warp> warps,
            List<Trigger> triggers, List<InitializationScript> initializationScripts, List<TriggerRelated> trigger2,
            string output) {
            new ZoneEntities {
                interactables = interactables,
                npcs = npCs,
                warps = warps,
                triggers = triggers,
                triggerRelatedEntries = trigger2,
                initializationScripts = initializationScripts
            }.serialize(output);
        }

        private void addNew<T>(List<T> target, T entry) {
            target.Add(entry);
        }

        private void removeSelected<T>(List<T> target, int selectedIndex) {
            if (selectedIndex != -1)
                target.RemoveAt(selectedIndex);
            else
                throw new Exception("You good? There's nothing to remove.");
        }

        public void addNewInteractable(Proxy entry) {
            addNew(interactables, entry);
        }

        public void addNewNpc(NPC entry) {
            addNew(npcs, entry);
        }

        public void addNewWarp(Warp entry) {
            addNew(warps, entry);
        }

        public void addNewTrigger(Trigger entry) {
            addNew(triggers, entry);
        }

        public void addNewInitScript(InitializationScript entry) {
            addNew(initializationScripts, entry);
        }

        public void removeSelectedInteractable(int selectedIndex) {
            removeSelected(interactables, selectedIndex);
        }

        public void removeSelectedNpc(int selectedIndex) {
            removeSelected(npcs, selectedIndex);
        }

        public void removeSelectedWarp(int selectedIndex) {
            removeSelected(warps, selectedIndex);
        }

        public void removeSelectedTrigger(int selectedIndex) {
            removeSelected(triggers, selectedIndex);
        }

        public void removeSelectedInitScript(int selectedIndex) {
            removeSelected(initializationScripts, selectedIndex);
        }

        public void parseOverworld(BinaryReader binary) {
            var fileSize = binary.ReadUInt32();
            var interactableCount = binary.ReadByte();
            var npcCount = binary.ReadByte();
            var warpCount = binary.ReadByte();
            var triggerCount = binary.ReadByte();

            for (var i = 0; i < interactableCount; ++i)
                interactables.Add(new Proxy(binary));

            for (var i = 0; i < npcCount; ++i)
                npcs.Add(new NPC(binary));

            for (var i = 0; i < warpCount; ++i)
                warps.Add(new Warp(binary));

            for (var i = 0; i < triggerCount; ++i)
                triggers.Add(new Trigger(binary));

            while (binary.PeekChar() != -1 && (binary.PeekChar() & 0x7FFF) != 0)
                initializationScripts.Add(new InitializationScript(binary));

            binary.BaseStream.Seek(0x2, SeekOrigin.Current);

            while (binary.PeekChar() != -1 && (binary.PeekChar() & 0x7FFF) != 0)
                triggerRelatedEntries.Add(new TriggerRelated(binary));
        }

        public void serialize(string path) {
            var binary = new BinaryWriter(File.OpenWrite(path));
            binary.Write((uint) (Proxy.Size * interactables.Count + NPC.size * npcs.Count +
                                 Warp.size * warps.Count + Trigger.size * triggers.Count + 0x4));
            binary.Write((byte) interactables.Count);
            binary.Write((byte) npcs.Count);
            binary.Write((byte) warps.Count);
            binary.Write((byte) triggers.Count);

            // Because BinaryFormatter is "unsafe"...
            interactables.ForEach(x => {
                binary.Write(x.Script);
                binary.Write(x.Condition);
                binary.Write(x.Interactibility);
                binary.Write(x.RailIndex);
                binary.Write(x.x);
                binary.Write(x.y);
                binary.Write(x.z * 0x10);
            });

            npcs.ForEach(x => {
                binary.Write(x.id);
                binary.Write(x.modelId);
                binary.Write(x.movementPermission);
                binary.Write(x.type);
                binary.Write(x.spawnFlag);
                binary.Write(x.scriptId);
                binary.Write(x.faceDirection);
                binary.Write(x.parameter);
                binary.Write(x.parameter2);
                binary.Write(x.parameter3);
                binary.Write(x.traversalWidth);
                binary.Write(x.traversalHeight);
                binary.Write(x.x);
                binary.Write(x.y);
                binary.Write(x.railSidePos);
                binary.Write(x.z);
            });

            warps.ForEach(x => {
                binary.Write(x.targetZone);
                binary.Write(x.targetWarp);
                binary.Write(x.contactDirection);
                binary.Write(x.transitionType);
                binary.Write(x.coordinateType);
                binary.Write(x.x);
                binary.Write(x.z);
                binary.Write(x.y);
                binary.Write(x.w);
                binary.Write(x.h);
                binary.Write(x.rail);
            });

            triggers.ForEach(x => {
                binary.Write(x.script);
                binary.Write(x.valueNeededForExecution);
                binary.Write(x.variable);
                binary.Write(x.unknown);
                binary.Write(x.unknown2);
                binary.Write(x.x);
                binary.Write(x.y);
                binary.Write(x.z);
                binary.Write(x.w);
                binary.Write(x.h);
                binary.Write(x.unknown3);
            });

            var sectionOffset = 0;

            initializationScripts.ForEach(x => {
                binary.Write(x.Type);
                binary.Write(x.ScriptIndex);
                binary.Write(x.Unknown);
                sectionOffset += 0x6;
            });

            binary.Write((ushort) 0x0);

            triggerRelatedEntries.ForEach(x => {
                binary.Write(x.variable);
                binary.Write(x.value);
                binary.Write(x.scriptId);
            });

            binary.Write((ushort) 0x0);

            binary.Close();
        }
    }
}