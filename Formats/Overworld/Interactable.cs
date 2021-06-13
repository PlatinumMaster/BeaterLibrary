using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BeaterLibrary.Formats.Furniture
{
	[Serializable()]
	public class Interactable
	{
		public ushort Script { get; set; }
		public ushort Condition { get; set; }
		public ushort Interactibility { get; set; }
		public ushort RailIndex { get; set; }
		public uint X { get; set; }
		public uint Y { get; set; }
		public int Z { get; set; }
		public static uint Size { get => 0x14; }

		public Interactable()
        {
			Script = 0;
			Condition = 0;
			Interactibility = 0;
			RailIndex = 0;
			X = 0;
			Y = 0;
			Z = 0;
		}

		public Interactable(BinaryReader b)
		{
			Script = b.ReadUInt16();
			Condition = b.ReadUInt16();
			Interactibility = b.ReadUInt16();
			RailIndex = b.ReadUInt16();
			X = b.ReadUInt32();
			Y = b.ReadUInt32();
			Z = b.ReadInt32() / 0x10;
		}
	}
}
