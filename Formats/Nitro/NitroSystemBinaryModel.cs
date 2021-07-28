using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BeaterLibrary.Formats.Nitro
{
    public class NitroSystemBinaryModel
    {
        private byte[] _data;

        public byte[] Data => _data;

        public NitroSystemBinaryModel()
        {
            _data = new byte[] { };
        }

        public NitroSystemBinaryModel(byte[] Binary)
        {
            if (Binary.Length < 0x10)
                throw new Exception("Invalid model.");

            if (Binary.Length != BitConverter.ToInt32(Binary.Skip(0x8).Take(0x4).ToArray(), 0))
                throw new Exception("Invalid model.");

            if (BitConverter.ToUInt16(Binary.Skip(0xE).Take(0x2).ToArray(), 0) != 1)
                throw new Exception("Sorry, but you must import a model that has only model data.");

            if (!Encoding.Default.GetString(Binary.Take(0x4).Select(x => x).ToArray()).Equals("BMD0"))
                throw new Exception("Invalid model.");

            _data = Binary;
        }

        public string Name
        {
            get => _data.Length == 0
                ? ""
                : Encoding.Default.GetString(_data.Skip(0x34).Take(0x10).Select(x => x).Where(x => x != 0).ToArray());
        }
    }
}