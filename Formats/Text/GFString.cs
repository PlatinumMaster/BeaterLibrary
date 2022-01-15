using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AvaloniaEdit.Text;
using BeaterLibrary.Parsing;
using OpenTK.Platform.Windows;

namespace BeaterLibrary.Formats.Text {
    public class GFString {
        private List<char> characters;
        private const ushort baseKey = 0x2983, compFlag = 0xF100, defaultCharacterSize = 0x10;
        private ushort decKey;
        private int currentIndex, bufferIndex, characterBuffer, mask, currentCharacterSize;

        private StringBuilder data { get; set; }
        public ushort charCount => (ushort) characters.Count;
        public bool isCompressed { get; private set; }
        public bool isEncrypted { get; private set; }

        public GFString(ushort step) {
            data = new StringBuilder();
            resetKey(step);
            characters = new List<char>();
            currentIndex = 0;
            bufferIndex = 0;
            characterBuffer = 0;
            mask = 0;
            currentCharacterSize = 0x10;
            isCompressed = false;
            isEncrypted = true;
        }
        
        public GFString(List<char> characters, ushort step) : this(step) {
            this.characters = characters;
            decryptCharacters();
        }

        public GFString(List<AbstractSyntaxNode> characters, ushort step) : this(step) {
            isCompressed = false;
            isEncrypted = false;
            parseAbstractString(characters);
            encryptCharacters();
        }

        private void handleCompresssionCharacter(List<char> characterList, char value, bool toCompress) {
            characterList.Add(value);
            bufferIndex -= toCompress ? defaultCharacterSize : currentCharacterSize;
            characterBuffer >>= toCompress ? defaultCharacterSize : currentCharacterSize;
        }
        
        public byte[] serialize() {
            byte[] buffer = new byte[0x2 * charCount];
            for (int Index = 0; Index < charCount; ++Index) {
                buffer[2 * Index] = (byte) (characters[Index] & 0xFF);
                buffer[2 * Index + 1] = (byte) (characters[Index] >> 0x8 & 0xFF);
            }
            return buffer;
        }
        
        public void decryptCharacters() {
            List<char> decompBuff = new List<char>();
            characters.ForEach(c => {
                char d = decryptCharacter(c);
                if (!isDefaultCharacterSize()) {
                    if (bufferIndex < currentCharacterSize) {
                        characterBuffer |= d << bufferIndex;
                        bufferIndex += 0x10;
                    }
                    while (decompBuff.Count < 1 || bufferIndex >= currentCharacterSize) {
                        char extracted;
                        if ((extracted = (char) (characterBuffer & mask)) != mask) {
                            addDecompressedChar(decompBuff, extracted);
                        } else {
                            setCharacterSize(0x10);
                            break;
                        }
                    }
                } else if (d == compFlag) {
                    isCompressed = true;
                    setCharacterSize(0x9);
                } else {
                    decompBuff.Add(d);
                }
            });
            isEncrypted = false;
            characters = decompBuff;
        }

        public void encryptCharacters() {
            List<char> compBuff = new List<char>();
            if (isCompressed) {
                compBuff.Add(encryptCharacter((char) compFlag)); // Compression character indicator
                setCharacterSize(0x9);
            }
            characters.ForEach(c => {
                if (!isDefaultCharacterSize()) {
                    characterBuffer |= (c & mask) << bufferIndex;
                    bufferIndex += currentCharacterSize;
                    if (bufferIndex >= defaultCharacterSize) {
                        addCompressedChar(compBuff, encryptCharacter((char) characterBuffer));
                    }
                } else {
                    compBuff.Add(encryptCharacter(c));
                }
            });
            if (!isDefaultCharacterSize()) {
                if (bufferIndex > 0) {
                    characterBuffer |= 0xFFFF << bufferIndex;
                    compBuff.Add(encryptCharacter((char)characterBuffer));
                }
            }
            compBuff.Add(encryptCharacter((char) 0xFFFF));
            isEncrypted = true;
            characters = compBuff;
        }
        
        private static string resolveCharacter(char character) {
            Dictionary<int, string> specialCharacterMap = new Dictionary<int, string>() {
                {0x2015, "―"},
                {0x246D, "♂"},
                {0x246E, "♀"},
                {0x2486, "🇵🇰"},
                {0x2487, "🇲🇳"},
                {0xFFFE, "\\n"},
                {0xFFFF, ""},
            };
            if (!specialCharacterMap.ContainsKey(character)) {
                return $"\\x{(ushort)character:X4}";
            }
            return specialCharacterMap[character];
        }
        
        // Utility
        private void resetKey(ushort step) => decKey = (ushort) (baseKey * step);
        public void keyStep() => decKey = (ushort) ((decKey << 0x3 | decKey >> 0xD) & 0xFFFF);
        private ushort maskDeriv(ushort shift) => (ushort) (Math.Pow(2, 0x10 - shift) - 1);
        private char handleEncryption(char c, bool encrypt) {
            char ret = (char) (encrypt ? decKey ^ c : c ^ decKey);
            keyStep();
            return ret;
        }

        private void addDecompressedChar(List<char> chars, char character) =>
            handleCompresssionCharacter(chars, character, false);        
        private void addCompressedChar(List<char> chars, char character) =>
            handleCompresssionCharacter(chars, character, true);
        private char decryptCharacter(char c) => handleEncryption(c, false);
        private char encryptCharacter(char c) => handleEncryption(c, true);
        private void resetCharacterSize() => setCharacterSize(defaultCharacterSize);
        private bool isDefaultCharacterSize() => defaultCharacterSize == currentCharacterSize;
        
        public override string ToString() {
            foreach (char character in characters) {
                data.Append(character > 0x14 && character < 0x100 ? character : resolveCharacter(character));
            }
            return data.ToString();
        }

        private void setCharacterSize(ushort newCharSize) {
            currentCharacterSize = newCharSize;
            mask = maskDeriv(newCharSize);
        }

        private void parseAbstractString(List<AbstractSyntaxNode> str) {
            characters = new List<char>();
            str.ForEach(x => {
                if (x.type == TextTokens.CompressionFlag) {
                    isCompressed = true;
                }
                else {
                    characters.Add(Convert.ToChar(x.value));
                }
            });
        }
    }
}