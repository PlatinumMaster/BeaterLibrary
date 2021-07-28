using BeaterLibrary.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BeaterLibrary.Formats.Text
{
    public class TextContainer
    {
        string DecryptCharacter(int encrypted)
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
                default:
                    if (encrypted > 0x14 && encrypted < 0xD800)
                        return Convert.ToChar(encrypted).ToString();
                    else
                        return $"\\x{encrypted:X4}";
            }
        }

        public string ParseText(string TextPath)
        {
            StringBuilder ParsedText = new StringBuilder();
            BinaryReader b = new BinaryReader(File.Open(TextPath, FileMode.Open));
            ushort nSections = b.ReadUInt16(), nEntries = b.ReadUInt16();
            uint sectionSize = b.ReadUInt32(), unknown = b.ReadUInt32(), sectionOffset = b.ReadUInt32();

            List<uint> TableOffsets = new List<uint>();
            List<ushort> CharacterCounts = new List<ushort>();
            List<ushort> Unknown2 = new List<ushort>();

            b.BaseStream.Position += 0x4;

            for (int i = 0; i < nEntries; i++)
            {
                TableOffsets.Add(b.ReadUInt32());
                CharacterCounts.Add(b.ReadUInt16());
                Unknown2.Add(b.ReadUInt16());
            }

            for (int i = 0; i < nEntries; i++)
            {
                List<ushort> TextCharacters = new List<ushort>();
                int Key;
                bool IsCompressed;
                b.BaseStream.Position = sectionOffset + TableOffsets[i];

                for (int j = 0; j < CharacterCounts[i]; j++)
                    TextCharacters.Add(b.ReadUInt16());

                Key = TextCharacters.Last() ^ 0xFFFF;

                for (int j = CharacterCounts[i] - 1; j >= 0; j--)
                {
                    TextCharacters[j] = (ushort) (TextCharacters[j] ^ Key);
                    Key = (Key >> 0x3 | Key << 0xD) & 0xFFFF;
                }

                IsCompressed = TextCharacters.First() == 0xF100;

                if (IsCompressed)
                {
                    List<ushort> DecompressedCharacters = new List<ushort>();
                    bool Parsing = true;
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

                ParsedText.AppendLine($"# STR_{i}");
                ParsedText.Append($"{(IsCompressed ? "!" : "")}[\"");

                for (int j = 0; j < TextCharacters.Count; j++)
                {
                    string Character = DecryptCharacter(TextCharacters[j]);
                    if (Character.Equals("SPECIAL"))
                    {
                        switch (DecryptCharacter(TextCharacters[++j]))
                        {
                            /*
                            case "븁":
                                // Clear character.
                                ++j;
                                Character = "\\c";
                                break;
                            case "븀":
                                // Scroll character.
                                ++j;
                                Character = "\\l";
                                break;
                            case "Ā":
                                // Trainer variable.
                                ++j;
                                Character = $"{{TRAINER_VAR {TextCharacters[j++]}, {TextCharacters[j]}}}";
                                break;
                            case "봂":
                                // Center directive.
                                ++j;
                                Character = "{CENTER}";
                                break;
                            case "븅":
                                // Pause text reading.
                                ++j;
                                Character = $"{{PAUSE {TextCharacters[j]}}}";
                                break;
                            case "ā":
                                // String buffer variable.
                                ++j;
                                Character = $"{{STRING_VAR {TextCharacters[j++]}, {TextCharacters[j]}}}";
                                break;
                            case "\\xFF00":
                                // Color change.
                                ++j;
                                Character = $"{{COLOR {TextCharacters[++j]}}}";
                                break;
                            */
                            default:
                                // I dunno.
                                Character = $"\\xF000{DecryptCharacter(TextCharacters[j])}";
                                break;
                        }
                    }

                    ParsedText.Append(Character);

                    if (Character.Equals("\\n"))
                        ParsedText.Append("\",\n\"");
                    else if (Character.Equals("\""))
                        ParsedText.Append("\\\"");
                }

                ParsedText.Append("\"]\n\n");
            }

            b.Close();
            return ParsedText.ToString();
        }

        public static void Serialize(string text, string output)
        {
            List<List<AbstractSyntaxNode>> TokenizedStrings = new TextTokenizer().Tokenize(text);
            BinaryWriter Binary = new BinaryWriter(File.Open(Path.GetFullPath(output), FileMode.OpenOrCreate));
            List<ushort> characterCounts = new List<ushort>();
            int mainKey = 0x7C89; // Encryption key.
            uint sectionSize = Convert.ToUInt32(4 + (8 * TokenizedStrings.Count));

            // Prepare strings for export.
            for (int i = 0; i < TokenizedStrings.Count; i++)
            {
                ushort characterCount = (ushort) TokenizedStrings[i].FindAll(x =>
                    x.Type == TextTokens.CompressionFlag || x.Type == TextTokens.TextCharacter ||
                    x.Type == TextTokens.StringTerminator || x.Type == TextTokens.ControlCharacter ||
                    x.Type == TextTokens.ByteSequence).Count;
                characterCounts.Add(characterCount);
                sectionSize += (uint) (0x2 * characterCount);
            }

            // Check if the string is compressed. If so, we need to do some work on the nodes.
            for (int i = 0; i < TokenizedStrings.Count; i++)
            {
                if (TokenizedStrings[i].FindAll(x => x.Type == TextTokens.CompressionFlag).Count > 0)
                {
                    throw new Exception(
                        "Sorry, 9-bit strings cannot be encoded at this time. Remove the compression operator and try again.");
                    // Compressed string, decompress it.
                    List<AbstractSyntaxNode> UncompressedNodes = TokenizedStrings[i].FindAll(x =>
                        x.Type == TextTokens.TextCharacter || x.Type == TextTokens.StringTerminator ||
                        x.Type == TextTokens.ControlCharacter || x.Type == TextTokens.ByteSequence);
                    List<AbstractSyntaxNode> CompressedNodes = new List<AbstractSyntaxNode>();
                    int container = 0, bit = 0;
                    for (int j = 0; j < UncompressedNodes.Count; ++j)
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
                        (ushort) (container | 0xFFFF << bit)));
                    TokenizedStrings[i] = CompressedNodes;
                }
            }


            Binary.Write((ushort) 0x1); // nSections. We only use 1. 
            Binary.Write((ushort) TokenizedStrings.Count); // Number of entries.
            Binary.Write(sectionSize); // Section size.
            Binary.Write(0); // Unknown.
            Binary.Write(0x10); // Section offset.

            // Begin writing the section.
            Binary.Write(sectionSize); // Section size.

            uint offset = (uint) (4 + (8 * TokenizedStrings.Count));
            for (int i = 0; i < TokenizedStrings.Count; i++)
            {
                Binary.Write(offset); // Offset.
                Binary.Write(characterCounts[i]);
                Binary.Write((ushort) 0x0);
                offset += (uint) (characterCounts[i] * 0x2);
            }

            foreach (List<AbstractSyntaxNode> Nodes in TokenizedStrings)
            {
                int key = mainKey;
                foreach (AbstractSyntaxNode Node in Nodes)
                {
                    switch (Node.Type)
                    {
                        case TextTokens.StringTerminator:
                        case TextTokens.TextCharacter:
                        case TextTokens.ByteSequence:
                        case TextTokens.CompressionFlag:
                            Binary.Write(Convert.ToUInt16(Convert.ToUInt16(Node.Value) ^ key));
                            key = (key << 0x3 | key >> 0xD) & 0xFFFF;
                            break;
                    }
                }

                mainKey += 0x2983;
                mainKey = mainKey > 0xFFFF ? mainKey - 0x10000 : mainKey;
            }

            // We're done... I hope.
            Binary.Close();
        }
    }
}