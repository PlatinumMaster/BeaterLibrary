using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts
{
    public class Script
    {
        private BinaryReader b;
        private readonly CommandsListHandler Handler;
        public List<int> Addresses { get; }
        public Dictionary<int, List<Command>> Scripts { get; }
        public Dictionary<int, List<Command>> Functions { get; }
        public Dictionary<int, List<Movement>> Movements { get; }

        public Script(string game)
        {
            Handler = new CommandsListHandler(game);
        }

        public Script(string path, string game)
        {
            // Initialize the script we will read from.
            b = new BinaryReader(File.OpenRead(path));

            Handler = new CommandsListHandler(game);
            Functions = new Dictionary<int, List<Command>>();
            Movements = new Dictionary<int, List<Movement>>();
            Addresses = GetScriptAddresses();
            Scripts = ReadScripts();
            // We are done reading from it.
            b.Close();
        }

        List<int> GetScriptAddresses()
        {
            List<int> addr = new List<int>();
            b.BaseStream.Position = 0;

            while (b.ReadUInt16() != 0xFD13)
            {
                b.BaseStream.Position -= 2;
                int address = b.ReadInt32() + (int)b.BaseStream.Position;
                if (!addr.Contains(address))
                    addr.Add(address);
            }
            return addr;
        }

        List<Movement> ReadMovement(int address)
        {
            List<Movement> movement = new List<Movement>();
            b.BaseStream.Position = address;

            while (true)
            {
                var idx = b.ReadUInt16();
                movement.Add(new Movement(idx.ToString(), idx, b.ReadUInt16()));

                if (b.ReadInt32() == 0xFE)
                    break;

                b.BaseStream.Position -= 0x4;
            }

            movement.Add(new Movement("0xFE", 0xFE, 0));
            return movement;
        }

        List<Command> ReadScript(int address)
        {
            b.BaseStream.Position = address;

            List<Command> script = new List<Command>();
            while (true)
            {
                var id = b.ReadUInt16();
                TryGetCommand(id, out var c);

                Console.WriteLine(c);

                foreach (Type t in c.Types)
                {
                    switch (t.Name)
                    {
                        case "Int32":
                            c.Parameters.Add(b.ReadInt32());
                            break;
                        case "UInt16":
                            c.Parameters.Add(b.ReadUInt16());
                            break;
                        case "Byte":
                            c.Parameters.Add(b.ReadByte());
                            break;
                        case "FX32":
                            c.Parameters.Add(b.ReadUInt32() / 4096);
                            break;
                        case "FX16":
                            c.Parameters.Add(b.ReadUInt16() / 4096);
                            break;
                    }
                }

                int originalPos = Convert.ToInt32(b.BaseStream.Position);
                int targetAddress = (c.HasFunction || c.HasMovement) ? originalPos + Convert.ToInt32(c.Parameters.Last()) : 0;

                if (c.HasFunction)
                {
                    if (!Functions.Keys.ToList().Contains(targetAddress))
                    {
                        Functions.Add(targetAddress, new List<Command>());
                        Functions[targetAddress] = ReadScript(targetAddress);
                        System.Diagnostics.Debug.WriteLine($"A function was detected at {targetAddress:X4}.");
                    }
                    c.Parameters[^1] = $"Function{Functions.Keys.ToList().IndexOf(targetAddress)}";
                }
                else if (c.HasMovement)
                {
                    if (!Movements.Keys.Contains(targetAddress))
                    {
                        Movements.Add(targetAddress, new List<Movement>());
                        Movements[targetAddress] = ReadMovement(targetAddress);
                        System.Diagnostics.Debug.WriteLine($"A movement was detected at {targetAddress}.");
                    }
                    c.Parameters[^1] = $"Movement{Movements.Keys.ToList().IndexOf(targetAddress)}";
                }

                b.BaseStream.Position = originalPos;
                script.Add(c);
                if (c.IsEnd)
                    break;
            }

            return script;
        }

        private bool TryGetCommand(ushort id, [NotNullWhen(true)] out Command? c)
        {
            if (!Handler.GetCommands().Contains(id))
            {
                Console.WriteLine($"WARNING: Unimplemented command: {id}");
                Console.WriteLine($"Position: {b.BaseStream.Position - 2}");
                throw new Exception("Script failed to decompile.");
                c = null;
                return false;
            }

            var def = Handler.GetCommand(id);
            c = new Command(def.Name, def.ID, def.HasFunction, def.HasMovement, def.IsEnd, def.dynamicParams, def.Types);
            return true;
        }

        Dictionary<int, List<Command>> ReadScripts() => Addresses.ToDictionary(Address => Address, ReadScript);

        public void Serialize(string script, string game)
        {
            using StreamWriter o = new StreamWriter(script);

            // Write the inclusion stuff.
            Console.Write("Writing header... ");
            o.WriteLine($".include \"{game}.s\"{Environment.NewLine}");

            // Write the header.
            o.WriteLine("Header:");
            for (int i = 0; i < Scripts.Count; i++)
                o.WriteLine($"\tscript Script{i}");
            o.WriteLine($"  EndHeader{Environment.NewLine}");

            Console.WriteLine("Writing functions... ");
            // Start writing the command section.
            for (int i = 0; i < Functions.Count; i++)
            {
                Console.WriteLine($"Writing function {i}...");
                o.WriteLine($"FunctionLabel Function{i}");
                foreach (Command cmd in Functions[Functions.ElementAt(i).Key])
                    o.WriteLine($"\t{cmd}");

                o.WriteLine(Environment.NewLine);
            }

            Console.WriteLine("Writing scripts... ");
            for (int i = 0; i < Scripts.Count; i++)
            {
                Console.WriteLine($"Writing script {i}...");
                o.WriteLine($"Script{i}:");
                foreach (Command cmd in Scripts[Scripts.ElementAt(i).Key])
                    o.WriteLine($"\t{cmd}");

                o.WriteLine(Environment.NewLine);
            }

            Console.WriteLine("Writing movements... ");
            // Finish off with movements.
            for (int i = 0; i < Movements.Count; i++)
            {
                Console.WriteLine($"Writing movement {i}...");
                o.WriteLine($"MovementLabel Movement{i}");
                for (int j = 0; j < Movements[Movements.ElementAt(i).Key].Count; j++)
                    o.WriteLine($"\t{Movements[Movements.ElementAt(i).Key][j]}");
                o.WriteLine(Environment.NewLine);
            }

            Console.WriteLine($"Script \"{script}\" has been written to text successfully.");
        }
    }
}
