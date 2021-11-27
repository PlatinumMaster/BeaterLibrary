using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeaterLibrary.Formats.Nitro;

namespace BeaterLibrary.Formats.Maps
{
    public class MapContainer
    {
        public enum MagicLabels
        {
            WB = 0x4257,
            GC = 0x4347,
            NG = 0x474E,
            RD = 0x4452
        }

        private ushort nSections;

        public MapContainer(ushort magic)
        {
            this.Magic = magic;
            Model = new NitroSystemBinaryModel();
            Permissions = new List<byte>();
            Permissions2 = new List<byte>();
            BuildingPositions = new List<byte>();
        }

        public MapContainer(BinaryReader Binary)
        {
            Model = new NitroSystemBinaryModel();
            Permissions = new List<byte>();
            Permissions2 = new List<byte>();
            BuildingPositions = new List<byte>();
            Magic = Binary.ReadUInt16();
            nSections = Binary.ReadUInt16();
            var ModelOffset = Binary.ReadUInt32();
            uint PermissionTableOffset = 0, PermissionTable2Offset = 0, BuildingPosOffset = 0, FileSize = 0;

            switch (ContainerType)
            {
                case MagicLabels.NG:
                    // Model and Building Positions
                    break;
                case MagicLabels.RD:
                case MagicLabels.WB:
                    // Model, Permission Table 1, and Building Positions
                    PermissionTableOffset = Binary.ReadUInt32();
                    break;
                case MagicLabels.GC:
                    // Model, Permission Table 1, Permission Table 2, and Building Positions
                    PermissionTableOffset = Binary.ReadUInt32();
                    PermissionTable2Offset = Binary.ReadUInt32();
                    break;
                default:
                    throw new Exception("Invalid magic");
            }

            BuildingPosOffset = Binary.ReadUInt32();
            FileSize = Binary.ReadUInt32();
            switch (ContainerType)
            {
                case MagicLabels.NG:
                    // Model and Building Positions
                    Model = new NitroSystemBinaryModel(Binary.ReadBytes((int) (BuildingPosOffset - ModelOffset)));
                    BuildingPositions = Binary.ReadBytes((int) (FileSize - BuildingPosOffset)).ToList();
                    break;
                case MagicLabels.RD:
                case MagicLabels.WB:
                    // Model, Permission Table 1, and Building Positions
                    Model = new NitroSystemBinaryModel(Binary.ReadBytes((int) (PermissionTableOffset - ModelOffset)));
                    Permissions = Binary.ReadBytes((int) (BuildingPosOffset - PermissionTableOffset)).ToList();
                    BuildingPositions = Binary.ReadBytes((int) (FileSize - BuildingPosOffset)).ToList();
                    break;
                case MagicLabels.GC:
                    // Model, Permission Table 1, Permission Table 2, and Building Positions
                    Model = new NitroSystemBinaryModel(Binary.ReadBytes((int) (PermissionTableOffset - ModelOffset)));
                    Permissions = Binary.ReadBytes((int) (PermissionTable2Offset - PermissionTableOffset)).ToList();
                    Permissions2 = Binary.ReadBytes((int) (BuildingPosOffset - PermissionTable2Offset)).ToList();
                    BuildingPositions = Binary.ReadBytes((int) (FileSize - BuildingPosOffset)).ToList();
                    break;
                default:
                    throw new Exception("Invalid number of sections.");
            }
        }

        public ushort Magic { get; set; }

        public MagicLabels ContainerType => (MagicLabels) Magic;

        public NitroSystemBinaryModel Model { get; set; }
        public List<byte> Permissions { get; set; }
        public List<byte> Permissions2 { get; set; }
        public List<byte> BuildingPositions { get; set; }

        public void Serialize(string path)
        {
            nSections = 2;
            nSections = Permissions.Count <= 0 ? nSections : (ushort) (nSections + 1);
            nSections = Permissions2.Count <= 0 ? nSections : (ushort) (nSections + 1);

            switch (nSections)
            {
                case 2:
                    Magic = (ushort) MagicLabels.NG;
                    break;
                case 3:
                    Magic = Permissions.Count == 0x6004 ? (ushort) MagicLabels.RD : (ushort) MagicLabels.WB;
                    break;
                case 4:
                    Magic = (ushort) MagicLabels.GC;
                    break;
            }

            using (var b = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
            {
                b.Write(Magic);
                b.Write(nSections);
                b.Write(0x4 * nSections + 0x8); // Location of model
                if (Permissions.Count > 0)
                    b.Write(0x4 * nSections + 0x8 + Model.Data.Length); // Location of Permissions table 1
                if (Permissions2.Count > 0)
                    b.Write(0x4 * nSections + 0x8 + Model.Data.Length +
                            Permissions.Count); // Location of Permissions table 2
                b.Write(0x4 * nSections + 0x8 + Model.Data.Length + Permissions.Count +
                        Permissions2.Count); // Location of building positions
                b.Write(0x4 * nSections + 0x8 + Model.Data.Length + Permissions.Count + Permissions2.Count +
                        BuildingPositions.Count); // file size
                b.Write(Model.Data);
                b.Write(Permissions.ToArray());
                b.Write(Permissions2.ToArray());
                b.Write(BuildingPositions.ToArray());
            }
        }
    }
}