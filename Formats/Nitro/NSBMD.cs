using System;
using System.Linq;
using System.Text;

namespace BeaterLibrary.Formats.Nitro {
    public class NSBMD {
        public NSBMD() {
            data = new byte[] { };
        }

        public NSBMD(byte[] binary) {
            if (binary.Length < 0x10)
                throw new Exception("Invalid model.");

            if (binary.Length != BitConverter.ToInt32(binary.Skip(0x8).Take(0x4).ToArray(), 0))
                throw new Exception("Invalid model.");

            if (BitConverter.ToUInt16(binary.Skip(0xE).Take(0x2).ToArray(), 0) != 1)
                throw new Exception("Sorry, but you must import a model that has only model data.");

            if (!Encoding.Default.GetString(binary.Take(0x4).Select(x => x).ToArray()).Equals("BMD0"))
                throw new Exception("Invalid model.");

            data = binary;
        }

        public byte[] data { get; }

        public string name =>
            data.Length == 0
                ? ""
                : Encoding.Default.GetString(data.Skip(0x34).Take(0x10).Select(x => x).Where(x => x != 0).ToArray());
    }
}