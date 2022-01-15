using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Maps {
    public class MapMatrix {
        public bool includeMapHeaders { get; set; }
        public List<MapMatrixRow> mapFilesMatrix { get; set; }
        public List<MapMatrixRow> mapHeadersMatrix { get; set; }
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
    }
}