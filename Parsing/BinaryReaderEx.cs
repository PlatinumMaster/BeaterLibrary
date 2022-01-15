using System.IO;
using System.Text;

namespace BeaterLibrary.Parsing {
    public class BinaryReaderEx : BinaryReader {
        public BinaryReaderEx(byte[] data) : base(new MemoryStream(data)) {
        }

        public BinaryReaderEx(Stream input) : base(input) {
        }

        public BinaryReaderEx(Stream input, Encoding encoding) : base(input, encoding) {
        }

        public BinaryReaderEx(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) {
        }
    }
}