using System.IO;

namespace BeaterLibrary.Formats.Scripts
{
    public class Action
    {
        public string Name { get; set; }
        private ushort Duration { get; set; }
        public ushort ID { get; set; }

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

        public override string ToString() =>
            ID == 0xFE ? Name : $"Action 0x{ID:X}, 0x{Duration:X}";
    }
}