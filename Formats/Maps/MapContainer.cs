using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeaterLibrary.Formats.Nitro;

namespace BeaterLibrary.Formats.Maps {
    public class MapContainer {
        public ushort magic { get; set; }
        public MagicLabels containerType {
            get;
            set;
        }

        public NitroSystemBinaryModel model { get; set; }
        public byte[] permissions { get; set; }
        public byte[] permissions2 { get; set; }
        public byte[] buildingPositions { get; set; }
        public enum MagicLabels {
            Wb = 0x4257,
            Gc = 0x4347,
            Ng = 0x474E,
            Rd = 0x4452
        }

        private ushort _nSections;

        public MapContainer(ushort magic) {
            this.magic = magic;
            model = new NitroSystemBinaryModel();
            permissions = new byte[] { };
            permissions2 = new byte[] { };
            buildingPositions = new byte[] { };
        }

        public MapContainer(byte[] data) {
            BinaryReader binary = new BinaryReader(new MemoryStream(data));
            model = new NitroSystemBinaryModel();
            permissions = new byte[] { };
            permissions2 = new byte[] { };
            buildingPositions = new byte[] { };
            magic = binary.ReadUInt16();
            _nSections = binary.ReadUInt16();
            var modelOffset = binary.ReadUInt32();
            containerType = (MagicLabels) magic;
            uint permissionTableOffset = 0, permissionTable2Offset = 0, buildingPosOffset = 0, fileSize = 0;

            switch (containerType) {
                case MagicLabels.Ng:
                    // Model and Building Positions
                    break;
                case MagicLabels.Rd:
                case MagicLabels.Wb:
                    // Model, Permission Table 1, and Building Positions
                    permissionTableOffset = binary.ReadUInt32();
                    break;
                case MagicLabels.Gc:
                    // Model, Permission Table 1, Permission Table 2, and Building Positions
                    permissionTableOffset = binary.ReadUInt32();
                    permissionTable2Offset = binary.ReadUInt32();
                    break;
                default:
                    throw new Exception("Invalid magic.");
            }

            buildingPosOffset = binary.ReadUInt32();
            fileSize = binary.ReadUInt32();
            switch (containerType) {
                case MagicLabels.Ng:
                    // Model and Building Positions
                    model = new NitroSystemBinaryModel(binary.ReadBytes((int) (buildingPosOffset - modelOffset)));
                    buildingPositions = binary.ReadBytes((int) (fileSize - buildingPosOffset));
                    break;
                case MagicLabels.Rd:
                case MagicLabels.Wb:
                    // Model, Permission Table 1, and Building Positions
                    model = new NitroSystemBinaryModel(binary.ReadBytes((int) (permissionTableOffset - modelOffset)));
                    permissions = binary.ReadBytes((int) (buildingPosOffset - permissionTableOffset));
                    buildingPositions = binary.ReadBytes((int) (fileSize - buildingPosOffset));
                    break;
                case MagicLabels.Gc:
                    // Model, Permission Table 1, Permission Table 2, and Building Positions
                    model = new NitroSystemBinaryModel(binary.ReadBytes((int) (permissionTableOffset - modelOffset)));
                    permissions = binary.ReadBytes((int) (permissionTable2Offset - permissionTableOffset));
                    permissions2 = binary.ReadBytes((int) (buildingPosOffset - permissionTable2Offset));
                    buildingPositions = binary.ReadBytes((int) (fileSize - buildingPosOffset));
                    break;
                default:
                    throw new Exception("Invalid number of sections.");
            }
        }

        public void updateContainerType() {
            if (permissions.Length == 0 && permissions2.Length == 0) {
                containerType = MagicLabels.Ng;
            } else if (permissions.Length != 0 || permissions2.Length != 0) {
                containerType = permissions.Length == 0x6004 ? MagicLabels.Rd : MagicLabels.Wb;
            } else {
                containerType = MagicLabels.Gc;
            }
        }

        public void serialize(string path) {
            _nSections = 2;
            _nSections = permissions.Length <= 0 ? _nSections : (ushort) (_nSections + 1);
            _nSections = permissions2.Length <= 0 ? _nSections : (ushort) (_nSections + 1);

            switch (_nSections) {
                case 2:
                    magic = (ushort) MagicLabels.Ng;
                    break;
                case 3:
                    magic = permissions.Length == 0x6004 ? (ushort) MagicLabels.Rd : (ushort) MagicLabels.Wb;
                    break;
                case 4:
                    magic = (ushort) MagicLabels.Gc;
                    break;
            }

            using (var b = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate))) {
                b.Write(magic);
                b.Write(_nSections);
                b.Write(0x4 * _nSections + 0x8); // Location of model
                if (permissions.Length > 0)
                    b.Write(0x4 * _nSections + 0x8 + model.data.Length); // Location of Permissions table 1
                if (permissions2.Length > 0)
                    b.Write(0x4 * _nSections + 0x8 + model.data.Length +
                            permissions.Length); // Location of Permissions table 2
                b.Write(0x4 * _nSections + 0x8 + model.data.Length + permissions.Length +
                        permissions2.Length); // Location of building positions
                b.Write(0x4 * _nSections + 0x8 + model.data.Length + permissions.Length + permissions2.Length +
                        buildingPositions.Length); // file size
                b.Write(model.data);
                b.Write(permissions.ToArray());
                b.Write(permissions2.ToArray());
                b.Write(buildingPositions.ToArray());
            }
        }
    }
}