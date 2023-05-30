using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Maps {
    public class MapMatrix {
        public bool IncludeMapHeaders { get; set; }
        public List<List<int>> MapFiles { get; set; }
        public List<List<int>> ZoneHeaders { get; set; }
        
        public MapMatrix() {
            MapFiles = new List<List<int>>();
            ZoneHeaders = new List<List<int>>();
        }
        
        public MapMatrix(byte[] data) {
            BinaryReader Binary = new BinaryReader(new MemoryStream(data));
            MapFiles = new List<List<int>>();
            ZoneHeaders = new List<List<int>>();
            IncludeMapHeaders = Binary.ReadInt32() is 1;
            
            int Width = Binary.ReadUInt16(), Height = Binary.ReadUInt16();
            
            for (int i = 0; i < Height; ++i) {
                List<int> Row = new List<int>();
                for (int j = 0; j < Width; ++j) {
                    Row.Add(Binary.ReadInt32());
                }
                MapFiles.Add(Row);
            }

            if (IncludeMapHeaders) {
                for (int i = 0; i < Height; ++i) {
                    List<int> Row = new List<int>();
                    for (int j = 0; j < Width; ++j) {
                        Row.Add(Binary.ReadInt32());
                    }
                    ZoneHeaders.Add(Row);
                }
            }
        }

        public void Resize(int NewHeight, int NewWidth) {
            if (NewWidth == 0 || NewHeight == 0) {
                // Reset matrix.
                Reset();
                return;
            }
            
            // Update height first.
            if (NewHeight != MapFiles.Count) {
                if (NewHeight > MapFiles.Count) {
                    while (NewHeight > MapFiles.Count) {
                        MapFiles.Add(new List<int>());
                    }
                    if (IncludeMapHeaders) {
                        while (NewHeight > ZoneHeaders.Count) {
                            ZoneHeaders.Add(new List<int>());
                        }
                    }
                } else {
                    while (NewHeight < MapFiles.Count) {
                        MapFiles.RemoveAt(MapFiles.Count - 1);
                    }
                    if (IncludeMapHeaders) {
                        while (NewHeight < ZoneHeaders.Count) {
                            ZoneHeaders.RemoveAt(ZoneHeaders.Count - 1);
                        }
                    }
                }
            }
            
            // TODO: Update width.
        }

        public void Reset() {
            this.MapFiles.Clear();
            if (IncludeMapHeaders) {
                this.ZoneHeaders.Clear();
            }
        }
        
        public void Serialize(string path) {
            BinaryWriter Binary = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
            Binary.Write((uint)(IncludeMapHeaders ? 1 : 0));
            if (MapFiles.Count > 0) {
                Binary.Write((ushort) MapFiles[0].Count);
            }
            Binary.Write((ushort)MapFiles.Count);
            MapFiles.ForEach(x => x.ForEach(Binary.Write));
            if (IncludeMapHeaders) {
                ZoneHeaders.ForEach(x => x.ForEach(Binary.Write));
            }
            Binary.Close();
        }
    }
}