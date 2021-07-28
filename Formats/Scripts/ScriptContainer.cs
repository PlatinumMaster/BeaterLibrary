using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts
{
    public class ScriptContainer
    {
        private BinaryReader Binary;
        private readonly CommandsListHandler Handler;
        public List<ScriptMethod> Scripts { get; }
        public List<Link<AnonymousScriptMethod>> Calls { get; }
        public List<Link<AnonymousScriptMethod>> Jumps { get; }
        public List<Link<Actions>> Actions { get; }
        
        public ScriptContainer(string Path, string ConfigurationPath, string Game, int[][] Plugins)
        {
            // Initialize the script we will read from.
            Binary = new BinaryReader(File.OpenRead(Path));
            Handler = new CommandsListHandler(Game, ConfigurationPath, Plugins);
            Scripts = new List<ScriptMethod>();
            Calls = new List<Link<AnonymousScriptMethod>>();
            Jumps = new List<Link<AnonymousScriptMethod>>();
            Actions = new List<Link<Actions>>();
            GetScriptAddresses().ForEach(x => Scripts.Add(new ScriptMethod(ReadCommands(x), x)));
            // We are done reading from it.
            Binary.Close();
        }

        public List<int> GetScriptAddresses()
        {
            List<int> Addresses = new List<int>();
            Binary.BaseStream.Position = 0;

            while (Binary.ReadUInt16() != 0xFD13)
            {
                Binary.BaseStream.Seek(-0x2, SeekOrigin.Current);
                int Address = Binary.ReadInt32() + (int) Binary.BaseStream.Position;
                if (!Addresses.Contains(Address))
                    Addresses.Add(Address);
            }
            return Addresses;
        }
        
        public List<Action> ReadActions(int Address)
        {
            List<Action> Action_Cmds = new List<Action>();
            Binary.BaseStream.Position = Address;
            Action_Cmds.Add(new Action(Binary));
            while (Action_Cmds.Last().ID != 0xFE)
                Action_Cmds.Add(new Action(Binary));
            Action_Cmds.Last().Name = "TerminateActionSequence";
            return Action_Cmds;
        }

        public List<Command> ReadCommands(int Address)
        {
            List<Command> Commands = new();
            List<Link<AnonymousScriptMethod>> LocalCalls = new(), LocalJumps = new();
            List<Link<Actions>> LocalActions = new();
            Binary.BaseStream.Position = Address;
            bool IsEnd = false;
            while (!IsEnd)
            {
                Command C = TryReadCommand();
                Commands.Add(C);
                switch (C.Type)
                {
                    case CommandTypes.Call:
                    case CommandTypes.ConditionalJump:
                    case CommandTypes.Jump:
                        BindLinkToCommand(C.Type is CommandTypes.Call ? LocalCalls : LocalJumps, (int) C.Parameters.Last(), C);
                        break;
                    case CommandTypes.Actions:
                        BindLinkToCommand(LocalActions, (int) C.Parameters.Last(), C);
                        break;
                    case CommandTypes.End:
                    case CommandTypes.Return:
                        IsEnd = !Jumps.Any(x => x.StartAddress == Binary.BaseStream.Position);
                        break;
                }
            }
            LocalActions.ForEach(x => {
                x.Data = new Actions(ReadActions(x.StartAddress));
                TryAddLink(Actions, x);
            });
            LocalCalls.ForEach(x => {
                x.Data = new AnonymousScriptMethod(ReadCommands(x.StartAddress), x.StartAddress);
                TryAddLink(Calls, x);
            });
            LocalJumps.ForEach(x => TryAddLink(Jumps, x));
            return Commands;
        }

        public Command TryReadCommand()
        {
            Command Cmd = TryGetCommandTemplate(Binary.ReadUInt16());
            foreach (Type T in Cmd.Types)
            {
                switch (T.Name)
                {
                    case "Int32":
                        Cmd.Parameters.Add(Binary.ReadInt32());
                        if (Command.FunctionTypes.Contains(Cmd.Type))
                            Cmd.Parameters[^1] = Convert.ToInt32(Binary.BaseStream.Position + (int) Cmd.Parameters[^1]);
                        break;
                    case "UInt16":
                        Cmd.Parameters.Add(Binary.ReadUInt16());
                        break;
                    case "Byte":
                        Cmd.Parameters.Add(Binary.ReadByte());
                        break;
                    case "FX32":
                        Cmd.Parameters.Add(Binary.ReadUInt32() / 0x1000);
                        break;
                    case "FX16":
                        Cmd.Parameters.Add(Binary.ReadUInt16() / 0x1000);
                        break;
                    default:
                        throw new Exception($"Invalid type \"{T.Name}\".");
                }
            }
            System.Diagnostics.Debug.WriteLine(Cmd);
            return Cmd;
        }
        
        private void BindLinkToCommand<T>(List<Link<T>> Target, int Address, Command C)
        {
            TryAddLink(Target, new Link<T> {
                StartAddress = Address,
            });
            C.Parameters[^1] = Target.Find(x => x.StartAddress == Address);
        }

        private void TryAddLink<T>(List<Link<T>> Target, Link<T> Link)
        {
            if (Target.All(x => x.StartAddress != Link.StartAddress))
                Target.Add(Link);
        }

        private Command TryGetCommandTemplate(ushort ID)
        {
            if (!Handler.GetCommands().Contains(ID))
                throw new Exception($"Unrecognized command ID: {ID} @ position {Binary.BaseStream.Position - 0x2}.");
            Command def = Handler.GetCommand(ID);
            return new Command(def.Name, def.ID, def.Type, def.Types);
        }
    }
}