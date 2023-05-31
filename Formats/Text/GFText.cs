using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BeaterLibrary.Parsing;

namespace BeaterLibrary.Formats.Text {
    public class GFText {
        private List<GFString> _strings;
        private const int DefaultStep = 0x3;

        public List<GFString> strings {
            get => _strings;
        }
        
        public GFText(byte[] data) {
            _strings = new List<GFString>();
            using (BinaryReader b = new BinaryReader(new MemoryStream(data))) {
                ParseTextContainer(b);
            }
        }

        private void ParseTextContainer(BinaryReader b) {
            var tableOffsets = new List<uint>();
            var characterCounts = new List<ushort>();
            var unknown2 = new List<ushort>();
            
            ushort nSections = b.ReadUInt16(), nEntries = b.ReadUInt16();
            uint sectionSize = b.ReadUInt32(), unknown = b.ReadUInt32(), sectionOffset = b.ReadUInt32();

            b.BaseStream.Seek(0x4, SeekOrigin.Current);
            
            for (var i = 0; i < nEntries; i++) {
                tableOffsets.Add(sectionOffset + b.ReadUInt32());
                characterCounts.Add(b.ReadUInt16());
                unknown2.Add(b.ReadUInt16());
            }
            
            tableOffsets.ForEach(offset => {
                List<char> characters = new List<char>();
                int index = tableOffsets.IndexOf(offset);
                b.BaseStream.Seek(offset, SeekOrigin.Begin);
                for (int Index = 0; Index < characterCounts[index]; ++Index) {
                    characters.Add((char) b.ReadUInt16());
                }
                _strings.Add(new GFString(characters, (ushort) (DefaultStep + index)));
            });
        }

        public string FetchTextAsString(bool includeComments, bool beautify) {
            StringBuilder s = new StringBuilder();
            for (int Index = 0; Index < strings.Count; ++Index) {
                if (includeComments) {
                    s.AppendLine($"# String Index {Index}");
                }
                
                if (beautify) {
                    s.Append($"{(strings[Index].isCompressed ? "!" : "")}[");
                    foreach (string str in strings[Index].ToString().Split("\\n")) {
                        s.Append($"\"{str.Replace("\"", "\\\"")}\",\n");
                    }
                    s.Length -= 2;
                    s.AppendLine("]");
                    s.AppendLine();
                }
                else {
                    s.AppendLine(strings[Index].ToString());
                }
            }
            return s.ToString();
        }

        public List<string> FetchTextAsStringArray() {
            List<string> s = new List<string>();
            for (int Index = 0; Index < strings.Count; ++Index) {
                s.Add(strings[Index].ToString());
            }
            return s;
        }

        public void Serialize(string text, string output) {
            var binary = new BinaryWriter(File.Open(output, FileMode.OpenOrCreate));
            var characterCounts = new List<ushort>();
            _strings = new List<GFString>();

            int textIndex = 0;
            new TextTokenizer().tokenize(text).ForEach(x => {
                List<AbstractSyntaxNode> importantTokens = x.FindAll(y => y.type is TextTokens.CompressionFlag or TextTokens.TextCharacter or TextTokens.ControlCharacter or TextTokens.ByteSequence);
                strings.Add(new GFString(importantTokens, (ushort)(DefaultStep + textIndex++)));
            });

            strings.ForEach(x => characterCounts.Add(x.charCount));
            
            uint sectionSize = 2 * characterCounts.Aggregate(Convert.ToUInt32(4 + 8 * strings.Count), (x, y) => x + y);
            binary.Write((ushort) 0x1); // nSections. We only use 1. 
            binary.Write((ushort) strings.Count); // Number of entries.
            binary.Write(sectionSize); // Section size.
            binary.Write(0); // Unknown.
            binary.Write(0x10); // Section offset.

            // Begin writing the section.
            binary.Write(sectionSize); // Section size.

            var offset = (uint) (4 + 8 * strings.Count);
            for (var i = 0; i < strings.Count; i++) {
                binary.Write(offset); // Offset.
                binary.Write(characterCounts[i]);
                binary.Write((ushort) 0x0);
                offset += (uint) (characterCounts[i] * 0x2);
            }
            
            strings.ForEach(x => binary.Write(x.serialize()));

            // We're done... I hope.
            binary.Close();
        }
    }
}