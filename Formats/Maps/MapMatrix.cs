using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Maps {
    public class MapMatrix {
        public bool includeMapHeaders { get; set; }
        public List<MapMatrixRow> mapFilesMatrix { get; set; }
        public List<MapMatrixRow> mapHeadersMatrix { get; set; }
        
        public MapMatrix() {
            mapFilesMatrix = new List<MapMatrixRow>();
            mapHeadersMatrix = new List<MapMatrixRow>();
        }
        
        public MapMatrix(byte[] data) {
            BinaryReader binary = new BinaryReader(new MemoryStream(data));
            mapFilesMatrix = new List<MapMatrixRow>();
            mapHeadersMatrix = new List<MapMatrixRow>();
            includeMapHeaders = binary.ReadInt32() is 1;
            int width = binary.ReadUInt16(), height = binary.ReadUInt16();
            for (int i = 0; i < height; ++i) {
                MapMatrixRow entries = new MapMatrixRow();
                for (int j = 0; j < width; ++j) {
                    entries.Add(new MapMatrixCell(binary.ReadInt32()));
                }
                mapFilesMatrix.Add(entries);
            }

            if (includeMapHeaders) {
                for (int i = 0; i < height; ++i) {
                    MapMatrixRow entries = new MapMatrixRow();
                    for (int j = 0; j < width; ++j) {
                        entries.Add(new MapMatrixCell(binary.ReadInt32()));
                    }
                    mapHeadersMatrix.Add(entries);
                }
            }
        }

        public void serialize(string path) {
            BinaryWriter binary = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
            binary.Write((uint)(includeMapHeaders ? 1 : 0));
            if (mapFilesMatrix.Count > 0) {
                binary.Write((ushort) mapFilesMatrix[0].Count);
            }
            binary.Write((ushort)mapFilesMatrix.Count);
            mapFilesMatrix.ForEach(x => {
                x.ForEach(e => binary.Write(e.value));
            });
            if (includeMapHeaders) {
                mapHeadersMatrix.ForEach(x => {
                    x.ForEach(e => binary.Write(e.value));
                });
            }
            binary.Close();
        }
    }
}