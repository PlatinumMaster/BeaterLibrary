using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BeaterLibrary.Parsing;

namespace BeaterLibrary.Formats.Text
{
    public class TextContainer
    {
        private static string DecryptCharacter(int encrypted)
        {
            switch (encrypted)
            {
                case 0xFFFF:
                    return "$";
                case 0x1FF:
                    return "#";
                case 0xFFFE:
                    return "\\n";
                case 0xF000:
                    return "SPECIAL";
                case 0x246D:
                    return "♂";
                case 0x246E:
                    return "♀";
                case 0x2486:
                    return "🇵🇰";
                case 0x2487:
                    return "🇲🇳";
                default:
                    if (encrypted > 0x14 && encrypted < 0x100)
                        return Convert.ToChar(encrypted).ToString();
                    else
                        return $"\\x{encrypted:X4}";
            }
        }

        public static string ParseText(string TextPath, bool IncludeComments, bool Beautify)
        {
            return ParseText(File.Open(TextPath, FileMode.Open), IncludeComments, Beautify);
        }

        public static string ParseText(Stream Buffer, bool IncludeComments, bool Beautify)
        {
            var ParsedText = new StringBuilder();
            var b = new BinaryReader(Buffer);
            ushort nSections = b.ReadUInt16(), nEntries = b.ReadUInt16();
            uint sectionSize = b.ReadUInt32(), unknown = b.ReadUInt32(), sectionOffset = b.ReadUInt32();

            var TableOffsets = new List<uint>();
            var CharacterCounts = new List<ushort>();
            var Unknown2 = new List<ushort>();

            b.BaseStream.Position += 0x4;

            for (var i = 0; i < nEntries; i++)
            {
                TableOffsets.Add(b.ReadUInt32());
                CharacterCounts.Add(b.ReadUInt16());
                Unknown2.Add(b.ReadUInt16());
            }

            for (var i = 0; i < nEntries; i++)
            {
                var TextCharacters = new List<ushort>();
                int Key;
                bool IsCompressed;
                b.BaseStream.Position = sectionOffset + TableOffsets[i];

                for (var j = 0; j < CharacterCounts[i]; j++)
                    TextCharacters.Add(b.ReadUInt16());

                Key = TextCharacters.Last() ^ 0xFFFF;

                for (var j = CharacterCounts[i] - 1; j >= 0; j--)
                {
                    TextCharacters[j] = (ushort) (TextCharacters[j] ^ Key);
                    Key = ((Key >> 0x3) | (Key << 0xD)) & 0xFFFF;
                }

                IsCompressed = TextCharacters.First() == 0xF100;

                if (IsCompressed)
                {
                    var DecompressedCharacters = new List<ushort>();
                    var Parsing = true;
                    int Container = 0, Bitshift = 0x0;
                    TextCharacters.RemoveAt(0); // Remove the compression flag "0xF100".                       
                    while (TextCharacters.Count > 0)
                    {
                        Container |= TextCharacters.First() << Bitshift;
                        TextCharacters.RemoveAt(0);
                        Bitshift += 0x10;
                        ushort c;
                        while (Parsing && Bitshift >= 0x9)
                        {
                            Bitshift -= 0x9;
                            c = Convert.ToUInt16(Container & 0x1FF);
                            if (c.Equals(0x1FF))
                            {
                                DecompressedCharacters.Add(0xFFFF);
                                Parsing = !Parsing;
                                break;
                            }

                            DecompressedCharacters.Add(c);
                            Container >>= 0x9;
                        }
                    }

                    TextCharacters = DecompressedCharacters;
                }

                if (IncludeComments)
                    ParsedText.AppendLine($"# STR_{i}");
                if (Beautify)
                    ParsedText.Append($"{(IsCompressed ? "!" : "")}[\"");

                for (var j = 0; j < TextCharacters.Count; j++)
                {
                    var Character = DecryptCharacter(TextCharacters[j]);
                    if (Character.Equals("SPECIAL"))
                        switch (DecryptCharacter(TextCharacters[++j]))
                        {
                            default:
                                // I dunno.
                                Character = $"\\xF000{DecryptCharacter(TextCharacters[j])}";
                                break;
                        }

                    if (Character.Equals("\\n") && Beautify)
                        ParsedText.Append("\\n\",\n\"");
                    else if (Character.Equals("\"") && Beautify)
                        ParsedText.Append("\\\"");
                    else if (Character.Equals("$") && !Beautify)
                        continue;

                    ParsedText.Append(Character);
                }

                if (Beautify)
                {
                    ParsedText.Append("\"]");
                    ParsedText.Append(Environment.NewLine);
                }

                ParsedText.Append(Environment.NewLine);
            }

            b.Close();
            return ParsedText.ToString();
        }

        public static void Serialize(string text, string output)
        {
            var TokenizedStrings = new TextTokenizer().Tokenize(text);
            var Binary = new BinaryWriter(File.Open(Path.GetFullPath(output), FileMode.OpenOrCreate));
            var characterCounts = new List<ushort>();
            var mainKey = 0x7C89; // Encryption key.
            var sectionSize = Convert.ToUInt32(4 + 8 * TokenizedStrings.Count);

            // Prepare strings for export.
            for (var i = 0; i < TokenizedStrings.Count; i++)
            {
                var characterCount = (ushort) TokenizedStrings[i].FindAll(x =>
                    x.Type == TextTokens.CompressionFlag || x.Type == TextTokens.TextCharacter ||
                    x.Type == TextTokens.StringTerminator || x.Type == TextTokens.ControlCharacter ||
                    x.Type == TextTokens.ByteSequence).Count;
                characterCounts.Add(characterCount);
                sectionSize += (uint) (0x2 * characterCount);
            }

            // Check if the string is compressed. If so, we need to do some work on the nodes.
            for (var i = 0; i < TokenizedStrings.Count; i++)
                if (TokenizedStrings[i].FindAll(x => x.Type == TextTokens.CompressionFlag).Count > 0)
                {
                    throw new Exception(
                        "Sorry, 9-bit strings cannot be encoded at this time. Remove the compression operator and try again.");
                    // Compressed string, decompress it.
                    var UncompressedNodes = TokenizedStrings[i].FindAll(x =>
                        x.Type == TextTokens.TextCharacter || x.Type == TextTokens.StringTerminator ||
                        x.Type == TextTokens.ControlCharacter || x.Type == TextTokens.ByteSequence);
                    var CompressedNodes = new List<AbstractSyntaxNode>();
                    int container = 0, bit = 0;
                    for (var j = 0; j < UncompressedNodes.Count; ++j)
                    {
                        container |= (ushort) UncompressedNodes[j].Value << bit;
                        bit += 0x9;
                        while (bit >= 0x10)
                        {
                            bit -= 0x10;
                            CompressedNodes.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, container & 0xFFFF));
                            container >>= 0x10;
                        }
                    }

                    CompressedNodes.Insert(0, new AbstractSyntaxNode(TextTokens.CompressionFlag, 0xF100));
                    CompressedNodes.Add(new AbstractSyntaxNode(TextTokens.ByteSequence,
                        (ushort) (container | (0xFFFF << bit))));
                    TokenizedStrings[i] = CompressedNodes;
                }


            Binary.Write((ushort) 0x1); // nSections. We only use 1. 
            Binary.Write((ushort) TokenizedStrings.Count); // Number of entries.
            Binary.Write(sectionSize); // Section size.
            Binary.Write(0); // Unknown.
            Binary.Write(0x10); // Section offset.

            // Begin writing the section.
            Binary.Write(sectionSize); // Section size.

            var offset = (uint) (4 + 8 * TokenizedStrings.Count);
            for (var i = 0; i < TokenizedStrings.Count; i++)
            {
                Binary.Write(offset); // Offset.
                Binary.Write(characterCounts[i]);
                Binary.Write((ushort) 0x0);
                offset += (uint) (characterCounts[i] * 0x2);
            }

            foreach (var Nodes in TokenizedStrings)
            {
                var key = mainKey;
                foreach (var Node in Nodes)
                    switch (Node.Type)
                    {
                        case TextTokens.StringTerminator:
                        case TextTokens.TextCharacter:
                        case TextTokens.ByteSequence:
                        case TextTokens.CompressionFlag:
                            Binary.Write(Convert.ToUInt16(Convert.ToUInt16(Node.Value) ^ key));
                            key = ((key << 0x3) | (key >> 0xD)) & 0xFFFF;
                            break;
                    }

                mainKey += 0x2983;
                mainKey = mainKey > 0xFFFF ? mainKey - 0x10000 : mainKey;
            }

            // We're done... I hope.
            Binary.Close();
        }
    }
}