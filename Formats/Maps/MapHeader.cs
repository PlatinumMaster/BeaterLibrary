using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Maps
{
    public class MapHeaders
    {
        public List<MapHeader> Headers { get; }

        public MapHeaders()
        {
            Headers = new List<MapHeader>();
        }

        public MapHeaders(string path) : this()
        {
            BinaryReader Binary = new BinaryReader(File.OpenRead(path));
            for (int i = 0; i < Binary.BaseStream.Length / 0x30; ++i)
                Headers.Add(new MapHeader(i, Binary));
            Binary.Close();
        }

        public void Serialize(string path)
        {
            BinaryWriter Binary = new BinaryWriter(File.OpenWrite(path));
            Headers.ForEach(x => x.Serialize(Binary));
            Binary.Close();
        }
    }

    public class MapHeader
    {
        public int Index { get; set; }
        public byte MapType { get; set; }
        public byte MapChange { get; set; }
        public ushort TextureContainerIndex { get; set; }
        public uint Fly_Y { get; set; }

        public int Fly_Z { get; set; }

        public uint Fly_X { get; set; }

        public int Unknown2 { get; set; }

        public ushort NameIcon { get; set; }

        public ushort MatrixCameraBoundary { get; set; }

        public ushort FlagsAndBackground { get; set; }

        public int CameraIndex { get; set; }

        public int UnknownFlag { get; set; }

        public int WeatherFlags { get; set; }

        public byte NameDisplayType { get; set; }

        public ushort NameIndex { get; set; }

        public ushort ParentZoneID { get; set; }

        public ushort ZoneID { get; set; }

        public ushort Unknown { get; set; }

        public ushort WildPokemonContainerIndex { get; set; }

        public ushort WinterBGM { get; set; }

        public ushort AutumnBGM { get; set; }

        public ushort SummerBGM { get; set; }

        public ushort SpringBGM { get; set; }

        public ushort TextContainerIndex { get; set; }

        public ushort InitializationScriptsIndex { get; set; }

        public ushort MapScriptsIndex { get; set; }

        public ushort MapMatrixIndex { get; set; }
        public MapHeader(int Index)
        {
            this.Index = Index;
        }

        public MapHeader(int Index, BinaryReader Binary) : this(Index)
        {
            MapType = Binary.ReadByte();
            MapChange = Binary.ReadByte();
            
            TextureContainerIndex = Binary.ReadUInt16();
            MapMatrixIndex = Binary.ReadUInt16();
            MapScriptsIndex = Binary.ReadUInt16();
            InitializationScriptsIndex = Binary.ReadUInt16();
            TextContainerIndex = Binary.ReadUInt16();

            SpringBGM = Binary.ReadUInt16();
            SummerBGM = Binary.ReadUInt16();
            AutumnBGM = Binary.ReadUInt16();
            WinterBGM = Binary.ReadUInt16();

            WildPokemonContainerIndex = Binary.ReadUInt16();
            Unknown = (ushort) ((WildPokemonContainerIndex >> 0xD) & 0x7);
            WildPokemonContainerIndex &= 0x1FFF;

            ZoneID = Binary.ReadUInt16();
            ParentZoneID = Binary.ReadUInt16();

            NameIndex = Binary.ReadUInt16();
            NameDisplayType = (byte) ((NameIndex >> 0xA) & 0x3F);
            NameIndex &= 0x3FF;

            var EnvironmentFlags = Binary.ReadUInt16();
            WeatherFlags = EnvironmentFlags & 0x3F;
            UnknownFlag = (EnvironmentFlags >> 0x6) & 0x7;
            CameraIndex = (EnvironmentFlags >> 0x9) & 0x7F;

            FlagsAndBackground = Binary.ReadUInt16();
            MatrixCameraBoundary = Binary.ReadUInt16();

            NameIcon = Binary.ReadUInt16();
            Unknown2 = (NameIcon >> 0xD) & 0x7;
            NameIcon &= 0x1FFF;

            Fly_X = Binary.ReadUInt32();
            Fly_Z = Binary.ReadInt32();
            Fly_Y = Binary.ReadUInt32();
        }


        public void Serialize(BinaryWriter Binary)
        {
            Binary.Write(MapType);
            Binary.Write(MapChange);
            Binary.Write(TextureContainerIndex);
            Binary.Write(MapMatrixIndex);
            Binary.Write(MapScriptsIndex);
            Binary.Write(InitializationScriptsIndex);
            Binary.Write(TextContainerIndex);
            Binary.Write(SpringBGM);
            Binary.Write(SummerBGM);
            Binary.Write(AutumnBGM);
            Binary.Write(WinterBGM);
            Binary.Write((ushort)(WildPokemonContainerIndex | (Unknown << 0xD)));
            Binary.Write(ZoneID);
            Binary.Write(ParentZoneID);
            Binary.Write((ushort)(NameIndex | (NameDisplayType << 0xA)));
            Binary.Write((ushort)(WeatherFlags | (UnknownFlag << 0x6) | (CameraIndex << 0x9)));
            Binary.Write(FlagsAndBackground);
            Binary.Write(MatrixCameraBoundary);
            Binary.Write((ushort)(NameIcon | (Unknown2 << 0xD)));
            Binary.Write(Fly_X);
            Binary.Write(Fly_Z);
            Binary.Write(Fly_Y);
        }

        public override string ToString() => $"Zone {Index}";
    }
}