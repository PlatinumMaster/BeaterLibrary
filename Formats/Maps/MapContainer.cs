using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeaterLibrary.Formats.Nitro;

namespace BeaterLibrary.Formats.Maps {
    public class MapContainer {
        public ushort Magic { get; set; }
        public MapContainerType ContainerType {
            get;
            set;
        }

        public NSBMD Model { get; set; }
        public byte[] Permissions { get; set; }
        public byte[] Permissions2 { get; set; }
        public byte[] BuildingPositions { get; set; }
        public enum MapContainerType {
            WB = 0x4257,
            GC = 0x4347,
            NG = 0x474E,
            RD = 0x4452
        }

        private ushort _nSections;

        public MapContainer(MapContainerType magic) {
            ContainerType = magic;
            Model = new NSBMD();
            Permissions = new byte[] { };
            Permissions2 = new byte[] { };
            BuildingPositions = new byte[] { };
        }

        public MapContainer(byte[] data) {
            BinaryReader Binary = new BinaryReader(new MemoryStream(data));
            Model = new NSBMD();
            Permissions = new byte[] { };
            Permissions2 = new byte[] { };
            BuildingPositions = new byte[] { };
            Magic = Binary.ReadUInt16();
            _nSections = Binary.ReadUInt16();
            var ModelOffset = Binary.ReadUInt32();
            ContainerType = (MapContainerType) Magic;
            uint PermissionTableOffset = 0, PermissionTable2Offset = 0, BuildingPosOffset = 0, FileSize = 0;

            switch (ContainerType) {
                case MapContainerType.NG:
                    // Model and Building Positions
                    break;
                case MapContainerType.RD:
                case MapContainerType.WB:
                    // Model, Permission Table 1, and Building Positions
                    PermissionTableOffset = Binary.ReadUInt32();
                    break;
                case MapContainerType.GC:
                    // Model, Permission Table 1, Permission Table 2, and Building Positions
                    PermissionTableOffset = Binary.ReadUInt32();
                    PermissionTable2Offset = Binary.ReadUInt32();
                    break;
                default:
                    throw new Exception("Invalid magic.");
            }

            BuildingPosOffset = Binary.ReadUInt32();
            FileSize = Binary.ReadUInt32();
            switch (ContainerType) {
                case MapContainerType.NG:
                    // Model and Building Positions
                    Model = new NSBMD(Binary.ReadBytes((int) (BuildingPosOffset - ModelOffset)));
                    BuildingPositions = Binary.ReadBytes((int) (FileSize - BuildingPosOffset));
                    break;
                case MapContainerType.RD:
                case MapContainerType.WB:
                    // Model, Permission Table 1, and Building Positions
                    Model = new NSBMD(Binary.ReadBytes((int) (PermissionTableOffset - ModelOffset)));
                    Permissions = Binary.ReadBytes((int) (BuildingPosOffset - PermissionTableOffset));
                    BuildingPositions = Binary.ReadBytes((int) (FileSize - BuildingPosOffset));
                    break;
                case MapContainerType.GC:
                    // Model, Permission Table 1, Permission Table 2, and Building Positions
                    Model = new NSBMD(Binary.ReadBytes((int) (PermissionTableOffset - ModelOffset)));
                    Permissions = Binary.ReadBytes((int) (PermissionTable2Offset - PermissionTableOffset));
                    Permissions2 = Binary.ReadBytes((int) (BuildingPosOffset - PermissionTable2Offset));
                    BuildingPositions = Binary.ReadBytes((int) (FileSize - BuildingPosOffset));
                    break;
                default:
                    throw new Exception("Invalid number of sections.");
            }
        }

        public void UpdateContainerType() {
            if (Permissions.Length == 0 && Permissions2.Length == 0) {
                ContainerType = MapContainerType.NG;
            } else if (Permissions.Length != 0 || Permissions2.Length != 0) {
                ContainerType = Permissions.Length == 0x6004 ? MapContainerType.RD : MapContainerType.WB;
            } else {
                ContainerType = MapContainerType.GC;
            }
        }

        public void Serialize(string path) {
            _nSections = 2;
            _nSections = Permissions.Length <= 0 ? _nSections : (ushort) (_nSections + 1);
            _nSections = Permissions2.Length <= 0 ? _nSections : (ushort) (_nSections + 1);

            switch (_nSections) {
                case 2:
                    Magic = (ushort) MapContainerType.NG;
                    break;
                case 3:
                    Magic = Permissions.Length == 0x6004 ? (ushort) MapContainerType.RD : (ushort) MapContainerType.WB;
                    break;
                case 4:
                    Magic = (ushort) MapContainerType.GC;
                    break;
            }

            using (var b = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate))) {
                b.Write(Magic);
                b.Write(_nSections);
                b.Write(0x4 * _nSections + 0x8); // Location of model
                if (Permissions.Length > 0)
                    b.Write(0x4 * _nSections + 0x8 + Model.Data.Length); // Location of Permissions table 1
                if (Permissions2.Length > 0)
                    b.Write(0x4 * _nSections + 0x8 + Model.Data.Length +
                            Permissions.Length); // Location of Permissions table 2
                b.Write(0x4 * _nSections + 0x8 + Model.Data.Length + Permissions.Length +
                        Permissions2.Length); // Location of building positions
                b.Write(0x4 * _nSections + 0x8 + Model.Data.Length + Permissions.Length + Permissions2.Length +
                        BuildingPositions.Length); // file size
                b.Write(Model.Data);
                b.Write(Permissions.ToArray());
                b.Write(Permissions2.ToArray());
                b.Write(BuildingPositions.ToArray());
            }
        }
    }
}