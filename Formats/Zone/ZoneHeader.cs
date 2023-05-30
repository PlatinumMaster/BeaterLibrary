using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Zone {
    public class ZoneHeaders {
        public ZoneHeaders() {
            headers = new List<ZoneHeader>();
        }

        public ZoneHeaders(byte[] data) : this() {
            var binary = new BinaryReader(new MemoryStream(data));
            readHeaders(binary);
            binary.Close();
        }

        public ZoneHeaders(string path) : this() {
            var binary = new BinaryReader(File.OpenRead(path));
            readHeaders(binary);
            binary.Close();
        }

        public List<ZoneHeader> headers { get; }

        private void readHeaders(BinaryReader binary) {
            for (var i = 0; i < binary.BaseStream.Length / 0x30; ++i)
                headers.Add(new ZoneHeader(i, binary));
        }

        public void serialize(string path) {
            var binary = new BinaryWriter(File.OpenWrite(path));
            headers.ForEach(x => x.serialize(binary));
            binary.Close();
        }

        public static void serialize(List<ZoneHeader> headers, string path) {
            var binary = new BinaryWriter(File.OpenWrite(path));
            headers.ForEach(x => x.serialize(binary));
            binary.Close();
        }
    }

    public class ZoneHeader {
        public ZoneHeader(int index) {
            this.index = index;
        }

        public ZoneHeader(int index, BinaryReader binary) : this(index) {
            mapType = binary.ReadByte();
            mapChange = binary.ReadByte();

            textureContainerIndex = binary.ReadUInt16();
            mapMatrixIndex = binary.ReadUInt16();
            mapScriptsIndex = binary.ReadUInt16();
            initializationScriptsIndex = binary.ReadUInt16();
            textContainerIndex = binary.ReadUInt16();

            springBgm = binary.ReadUInt16();
            summerBgm = binary.ReadUInt16();
            autumnBgm = binary.ReadUInt16();
            winterBgm = binary.ReadUInt16();

            wildPokemonContainerIndex = binary.ReadUInt16();
            unknown = (ushort) ((wildPokemonContainerIndex >> 0xD) & 0x7);
            wildPokemonContainerIndex &= 0x1FFF;

            zoneId = binary.ReadUInt16();
            parentZoneId = binary.ReadUInt16();

            nameIndex = binary.ReadUInt16();
            nameDisplayType = (byte) ((nameIndex >> 0xA) & 0x3F);
            nameIndex &= 0x3FF;

            var environmentFlags = binary.ReadUInt16();
            weatherFlags = environmentFlags & 0x3F;
            unknownFlag = (environmentFlags >> 0x6) & 0x7;
            cameraIndex = (environmentFlags >> 0x9) & 0x7F;

            flagsAndBackground = binary.ReadUInt16();
            smth = flagsAndBackground & 0x1F;
            battleBackground = (flagsAndBackground >> 5) & 0xF;
            canBike = Convert.ToBoolean((flagsAndBackground >> 10) & 1);
            canRun = Convert.ToBoolean((flagsAndBackground >> 11) & 1);
            canEscapeRope = Convert.ToBoolean((flagsAndBackground >> 12) & 1);
            canFly = Convert.ToBoolean((flagsAndBackground >> 13) & 1);
            isBgmChangeEnabled = Convert.ToBoolean((flagsAndBackground >> 14) & 1);
            unknownBool = Convert.ToBoolean((flagsAndBackground >> 15) & 1);

            matrixCameraBoundary = binary.ReadUInt16();
            nameIcon = binary.ReadUInt16();
            unknown2 = (nameIcon >> 0xD) & 0x7;
            nameIcon &= 0x1FFF;

            flyX = binary.ReadUInt32();
            flyZ = binary.ReadInt32();
            flyY = binary.ReadUInt32();
        }

        public int battleBackground { get; set; }

        public int smth { get; set; }
        public bool canBike { get; set; }
        public bool canRun { get; set; }
        public bool canEscapeRope { get; set; }
        public bool canFly { get; set; }
        public bool isBgmChangeEnabled { get; set; }
        public bool unknownBool { get; set; }
        public int index { get; set; }
        public byte mapType { get; set; }
        public byte mapChange { get; set; }
        public ushort textureContainerIndex { get; set; }
        public uint flyY { get; set; }
        public int flyZ { get; set; }

        public uint flyX { get; set; }
        public int unknown2 { get; set; }

        public ushort nameIcon { get; set; }

        public ushort matrixCameraBoundary { get; set; }

        public ushort flagsAndBackground { get; set; }

        public int cameraIndex { get; set; }

        public int unknownFlag { get; set; }

        public int weatherFlags { get; set; }

        public byte nameDisplayType { get; set; }

        public ushort nameIndex { get; set; }

        public ushort parentZoneId { get; set; }

        public ushort zoneId { get; set; }

        public ushort unknown { get; set; }

        public ushort wildPokemonContainerIndex { get; set; }

        public ushort winterBgm { get; set; }

        public ushort autumnBgm { get; set; }

        public ushort summerBgm { get; set; }

        public ushort springBgm { get; set; }

        public ushort textContainerIndex { get; set; }

        public ushort initializationScriptsIndex { get; set; }

        public ushort mapScriptsIndex { get; set; }

        public ushort mapMatrixIndex { get; set; }


        public void serialize(BinaryWriter binary) {
            binary.Write(mapType);
            binary.Write(mapChange);
            binary.Write(textureContainerIndex);
            binary.Write(mapMatrixIndex);
            binary.Write(mapScriptsIndex);
            binary.Write(initializationScriptsIndex);
            binary.Write(textContainerIndex);
            binary.Write(springBgm);
            binary.Write(summerBgm);
            binary.Write(autumnBgm);
            binary.Write(winterBgm);
            binary.Write((ushort) (wildPokemonContainerIndex | (unknown << 0xD)));
            binary.Write(zoneId);
            binary.Write(parentZoneId);
            binary.Write((ushort) (nameIndex | (nameDisplayType << 0xA)));
            binary.Write((ushort) (weatherFlags | (unknownFlag << 0x6) | (cameraIndex << 0x9)));
            binary.Write((ushort) ((smth & 0x1F) | ((battleBackground & 0xFF) << 5) |
                                   ((Convert.ToUInt32(canBike) & 1) << 10) |
                                   ((Convert.ToUInt32(canRun) & 1) << 11) |
                                   ((Convert.ToUInt32(canEscapeRope) & 1) << 12) |
                                   ((Convert.ToUInt32(canFly) & 1) << 13) |
                                   ((Convert.ToUInt32(isBgmChangeEnabled) & 1) << 14) |
                                   ((Convert.ToUInt32(unknownBool) & 1) << 15)));
            binary.Write(matrixCameraBoundary);
            binary.Write((ushort) (nameIcon | (unknown2 << 0xD)));
            binary.Write(flyX);
            binary.Write(flyZ);
            binary.Write(flyY);
        }

        public override string ToString() {
            return $"Zone {index}";
        }
    }
}