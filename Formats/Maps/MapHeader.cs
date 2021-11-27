using System;
using System.Collections.Generic;
using System.IO;

namespace BeaterLibrary.Formats.Maps
{
    public class MapHeaders
    {
        public MapHeaders()
        {
            Headers = new List<MapHeader>();
        }

        public MapHeaders(byte[] Data) : this()
        {
            var Binary = new BinaryReader(new MemoryStream(Data));
            ReadHeaders(Binary);
            Binary.Close();
        }

        public MapHeaders(string path) : this()
        {
            var Binary = new BinaryReader(File.OpenRead(path));
            ReadHeaders(Binary);
            Binary.Close();
        }

        public List<MapHeader> Headers { get; }

        private void ReadHeaders(BinaryReader Binary)
        {
            for (var i = 0; i < Binary.BaseStream.Length / 0x30; ++i)
                Headers.Add(new MapHeader(i, Binary));
        }

        public void Serialize(string path)
        {
            var Binary = new BinaryWriter(File.OpenWrite(path));
            Headers.ForEach(x => x.Serialize(Binary));
            Binary.Close();
        }

        public static void Serialize(List<MapHeader> Headers, string path)
        {
            var Binary = new BinaryWriter(File.OpenWrite(path));
            Headers.ForEach(x => x.Serialize(Binary));
            Binary.Close();
        }
    }

    public class MapHeader
    {
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
            Smth = FlagsAndBackground & 0x1F;
            BattleBackground = (FlagsAndBackground >> 5) & 0xF;
            CanBike = Convert.ToBoolean((FlagsAndBackground >> 10) & 1);
            CanRun = Convert.ToBoolean((FlagsAndBackground >> 11) & 1);
            CanEscapeRope = Convert.ToBoolean((FlagsAndBackground >> 12) & 1);
            CanFly = Convert.ToBoolean((FlagsAndBackground >> 13) & 1);
            IsBGMChangeEnabled = Convert.ToBoolean((FlagsAndBackground >> 14) & 1);
            UnknownBool = Convert.ToBoolean((FlagsAndBackground >> 15) & 1);
            
            MatrixCameraBoundary = Binary.ReadUInt16();
            NameIcon = Binary.ReadUInt16();
            Unknown2 = (NameIcon >> 0xD) & 0x7;
            NameIcon &= 0x1FFF;

            Fly_X = Binary.ReadUInt32();
            Fly_Z = Binary.ReadInt32();
            Fly_Y = Binary.ReadUInt32();
        }

        public int BattleBackground { get; set; }

        public int Smth { get; set; }
        public bool CanBike { get; set; }
        public bool CanRun { get; set; }
        public bool CanEscapeRope { get; set; }
        public bool CanFly { get; set; }
        public bool IsBGMChangeEnabled { get; set; }
        public bool UnknownBool { get; set; }
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
            Binary.Write((ushort) (WildPokemonContainerIndex | (Unknown << 0xD)));
            Binary.Write(ZoneID);
            Binary.Write(ParentZoneID);
            Binary.Write((ushort) (NameIndex | (NameDisplayType << 0xA)));
            Binary.Write((ushort) (WeatherFlags | (UnknownFlag << 0x6) | (CameraIndex << 0x9)));
            Binary.Write((ushort)((Smth & 0x1F) | ((BattleBackground & 0xFF) << 5) |
                         ((Convert.ToUInt32(CanBike) & 1) << 10) |
                         ((Convert.ToUInt32(CanRun) & 1) << 11) |
                         ((Convert.ToUInt32(CanEscapeRope) & 1) << 12) |
                         ((Convert.ToUInt32(CanFly) & 1) << 13) |
                         ((Convert.ToUInt32(IsBGMChangeEnabled) & 1) << 14) |
                         ((Convert.ToUInt32(UnknownBool) & 1) << 15)));
            Binary.Write(MatrixCameraBoundary);
            Binary.Write((ushort) (NameIcon | (Unknown2 << 0xD)));
            Binary.Write(Fly_X);
            Binary.Write(Fly_Z);
            Binary.Write(Fly_Y);
        }

        public override string ToString()
        {
            return $"Zone {Index}";
        }
    }
}