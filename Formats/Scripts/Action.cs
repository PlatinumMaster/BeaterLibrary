using System.IO;

namespace BeaterLibrary.Formats.Scripts {
    public class Action {
        public Action(BinaryReader binary) {
            id = binary.ReadUInt16();
            duration = binary.ReadUInt16();
            name = id.ToString();
        }

        public Action(string name, ushort id, ushort duration) {
            this.name = name;
            this.id = id;
            this.duration = duration;
        }

        public string name { get; set; }
        private ushort duration { get; }
        public ushort id { get; set; }

        public override string ToString() {
            return id == 0xFE ? name : $"Action 0x{id:X}, 0x{duration:X}";
        }
    }
}