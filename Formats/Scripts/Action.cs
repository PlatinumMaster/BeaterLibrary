using System.IO;

namespace BeaterLibrary.Formats.Scripts
{
    public class Action
    {
        public Action(BinaryReader Binary)
        {
            ID = Binary.ReadUInt16();
            Duration = Binary.ReadUInt16();
            Name = ID.ToString();
        }

        public Action(string name, ushort id, ushort duration)
        {
            Name = name;
            ID = id;
            Duration = duration;
        }

        public string Name { get; set; }
        private ushort Duration { get; }
        public ushort ID { get; set; }

        public override string ToString()
        {
            return ID == 0xFE ? Name : $"Action 0x{ID:X}, 0x{Duration:X}";
        }
    }
}