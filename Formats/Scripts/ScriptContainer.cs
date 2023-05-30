using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts {
    public class ScriptContainer {
        private readonly BinaryReader _binary;
        private readonly CommandsListHandler _handler;
        public List<ScriptMethod> Scripts { get; }
        public List<Link<AnonymousScriptMethod>> Calls { get; }
        public List<Link<AnonymousScriptMethod>> Jumps { get; }
        public List<Link<Actions>> Actions { get; }
        
        public ScriptContainer(byte[] data, string configurationPath, string game, int plugin) {
            // Initialize the script we will read from.
            _binary = new BinaryReader(new MemoryStream(data));
            _handler = new CommandsListHandler(game, configurationPath, plugin);
            Scripts = new List<ScriptMethod>();
            Calls = new List<Link<AnonymousScriptMethod>>();
            Jumps = new List<Link<AnonymousScriptMethod>>();
            Actions = new List<Link<Actions>>();
            GetScriptAddresses().ForEach(x => Scripts.Add(new ScriptMethod(ReadCommands(x), x)));
            // We are done reading from it.
            _binary.Close();
        }

        public List<int> GetScriptAddresses() {
            var addresses = new List<int>();
            _binary.BaseStream.Position = 0;

            while (_binary.ReadUInt16() != 0xFD13) {
                _binary.BaseStream.Seek(-0x2, SeekOrigin.Current);
                var address = _binary.ReadInt32() + (int) _binary.BaseStream.Position;
                if (address >= _binary.BaseStream.Length) {
                    break;
                }
                if (!addresses.Contains(address)) {
                    addresses.Add(address);
                }
            }

            return addresses;
        }

        public List<Action> ReadActions(int Address) {
            List<Action> ActionCmds = new List<Action>();
            _binary.BaseStream.Position = Address;
            ActionCmds.Add(new Action(_binary));
            while (ActionCmds.Last().Id != 0xFE) ActionCmds.Add(new Action(_binary));
            ActionCmds.Last().Name = "TerminateActionSequence";
            return ActionCmds;
        }

        public List<Command> ReadCommands(int Address) {
            List<Command> Commands = new List<Command>();
            List<Link<AnonymousScriptMethod>> LocalCalls = new List<Link<AnonymousScriptMethod>>(), LocalJumps = new List<Link<AnonymousScriptMethod>>();
            List<Link<Actions>> LocalActions = new List<Link<Actions>>();
            _binary.BaseStream.Seek(Address, SeekOrigin.Begin);
            
            bool End = false;
            while (!End) {
                Command c = TryReadCommand();
                Commands.Add(c);
                switch (c.Type) {
                    case CommandTypes.Call:
                    case CommandTypes.ConditionalJump:
                    case CommandTypes.Jump:
                        BindLinkToCommand(c.Type is CommandTypes.Call ? LocalCalls : LocalJumps, (int) c.Parameters.Last(), c);
                        break;
                    case CommandTypes.ActionSequence:
                        BindLinkToCommand(LocalActions, (int) c.Parameters.Last(), c);
                        break;
                    case CommandTypes.End:
                    case CommandTypes.Return:
                        End = Jumps.All(x => x.StartAddress != _binary.BaseStream.Position);
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

        public Command TryReadCommand() {
            Command Cmd = TryGetCommandTemplate(_binary.ReadUInt16());
            foreach (Type T in Cmd.Types) {
                switch (T.Name) {
                    case "Int32":
                        Cmd.Parameters.Add(_binary.ReadInt32());
                        if (Command.FunctionTypes.Contains(Cmd.Type)) {
                            Cmd.Parameters[^1] = Convert.ToInt32(_binary.BaseStream.Position + (int) Cmd.Parameters[^1]);
                        }
                        break;
                    case "UInt16":
                        Cmd.Parameters.Add(_binary.ReadUInt16());
                        break;
                    case "Byte":
                        Cmd.Parameters.Add(_binary.ReadByte());
                        break;
                    case "FX32":
                        Cmd.Parameters.Add(_binary.ReadUInt32() / 0x1000);
                        break;
                    case "FX16":
                        Cmd.Parameters.Add(_binary.ReadUInt16() / 0x1000);
                        break;
                    default:
                        throw new Exception($"Invalid type \"{T.Name}\".");
                }
            }
            return Cmd;
        }

        private void BindLinkToCommand<T>(List<Link<T>> Target, int Address, Command C) {
            TryAddLink(Target, new Link<T> {
                StartAddress = Address
            });
            C.Parameters[^1] = Target.Find(x => x.StartAddress == Address);
        }

        private void TryAddLink<T>(List<Link<T>> Target, Link<T> Link) {
            if (Target.All(x => x.StartAddress != Link.StartAddress)) {
                Target.Add(Link);
            }
        }

        private Command TryGetCommandTemplate(ushort ID) {
            if (!_handler.GetCommands().Contains(ID)) {
                throw new Exception($"Unrecognized command ID: {ID} @ position {_binary.BaseStream.Position - 0x2}.");
            }
            Command def = _handler.GetCommand(ID);
            return new Command(def.Name, def.ID, def.Type, def.Types, def.ParameterNames, def.ParameterDesc);
        }
    }
}