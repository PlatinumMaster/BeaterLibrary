using System.IO;

namespace BeaterLibrary.Formats.Scripts {
    public class Action {
        public Action(BinaryReader binary) {
            Id = binary.ReadUInt16();
            Duration = binary.ReadUInt16();
            Name = Id.ToString();
        }

        public Action(string name, ushort id, ushort duration) {
            Name = name;
            Id = id;
            Duration = duration;
        }

        public string Name { get; set; }
        private ushort Duration { get; }
        public ushort Id { get; set; }

        public override string ToString() {
            return Id == 0xFE ? Name : $"Action 0x{Id:X}, 0x{Duration:X}";
        }
    }
}