using System;
using System.Collections.Generic;
using System.IO;

public class MapContainer
{
    uint magic = 0x00034257; // WB
    public List<byte> Model { get; set; }
    public List<byte> Permissions { get; set; }
    public List<byte> BuildingPositions { get; set; }
    public MapContainer()
    {
        Model = new List<byte>();
        Permissions = new List<byte>();
        BuildingPositions = new List<byte>();
    }

    public MapContainer(List<byte> Model, List<byte> Permissions, List<byte> BuildingPositions)
    {
        this.Model = Model;
        this.Permissions = Permissions;
        this.BuildingPositions = BuildingPositions;
    }

    public void Serialize(string path)
    {
        using (BinaryWriter b = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate)))
        {
            b.Write(magic);
            b.Write(0x14); // Location of model
            b.Write(0x14 + Model.Count); // Location of Perms
            b.Write(0x14 + Model.Count + Permissions.Count); // Location of building positions
            b.Write(0x14 + Model.Count + Permissions.Count + BuildingPositions.Count); // file size
            b.Write(Model.ToArray());
            b.Write(Permissions.ToArray());
            b.Write(BuildingPositions.ToArray());
        }
    }
}

