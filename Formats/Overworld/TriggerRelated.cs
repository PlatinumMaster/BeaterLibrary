using System.IO;

namespace BeaterLibrary.Formats.Scripts
{
    public class   TriggerRelated
    {
        public TriggerRelated()
        {
            Variable = 0;
            Value = 0;
            ScriptID = 0;
        }

        public TriggerRelated(BinaryReader Binary)
        {
            Variable = Binary.ReadUInt16();
            Value = Binary.ReadUInt16();
            ScriptID = Binary.ReadUInt16();
        }

        public ushort Variable { get; set; }
        public ushort Value { get; set; }
        public ushort ScriptID { get; set; }
    }
}