using System.IO;
using BeaterLibrary.Formats.Overworld;

namespace BeaterLibrary.Formats.Scripts {
    public class TriggerRelated : FieldObject {
        public TriggerRelated() {
            variable = 0;
            value = 0;
            scriptId = 0;
        }

        public TriggerRelated(BinaryReader binary) {
            variable = binary.ReadUInt16();
            value = binary.ReadUInt16();
            scriptId = binary.ReadUInt16();
        }

        public ushort variable { get; set; }
        public ushort value { get; set; }
        public ushort scriptId { get; set; }
    }
}