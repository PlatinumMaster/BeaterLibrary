using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;

namespace BeaterLibrary.Formats.Scripts {
    public class ScriptContainer {
        private readonly BinaryReader _binary;
        private readonly CommandsListHandler _handler;
        public List<ScriptMethod> scripts { get; }
        public List<Link<AnonymousScriptMethod>> calls { get; }
        public List<Link<AnonymousScriptMethod>> jumps { get; }
        public List<Link<Actions>> actions { get; }
        
        public ScriptContainer(byte[] data, string configurationPath, string game, int plugin) {
            // Initialize the script we will read from.
            _binary = new BinaryReader(new MemoryStream(data));
            _handler = new CommandsListHandler(game, configurationPath, plugin);
            scripts = new List<ScriptMethod>();
            calls = new List<Link<AnonymousScriptMethod>>();
            jumps = new List<Link<AnonymousScriptMethod>>();
            actions = new List<Link<Actions>>();
            getScriptAddresses().ForEach(x => scripts.Add(new ScriptMethod(readCommands(x), x)));
            // We are done reading from it.
            _binary.Close();
        }
        
        public ScriptContainer(string path, string configurationPath, string game, int plugin) : this(
            File.ReadAllBytes(path), configurationPath, game, plugin) {
        }

        public List<int> getScriptAddresses() {
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

        public List<Action> readActions(int address) {
            var actionCmds = new List<Action>();
            _binary.BaseStream.Position = address;
            actionCmds.Add(new Action(_binary));
            while (actionCmds.Last().id != 0xFE) actionCmds.Add(new Action(_binary));
            actionCmds.Last().name = "TerminateActionSequence";
            return actionCmds;
        }

        public List<Command> readCommands(int address) {
            List<Command> commands = new();
            List<Link<AnonymousScriptMethod>> localCalls = new(), localJumps = new();
            List<Link<Actions>> localActions = new();
            _binary.BaseStream.Position = address;
            var isEnd = false;
            while (!isEnd) {
                var c = tryReadCommand();
                commands.Add(c);
                Console.WriteLine(c.name);
                switch (c.type) {
                    case CommandTypes.Call:
                    case CommandTypes.ConditionalJump:
                    case CommandTypes.Jump:
                        bindLinkToCommand(c.type is CommandTypes.Call ? localCalls : localJumps,
                            (int) c.parameters.Last(), c);
                        break;
                    case CommandTypes.ActionSequence:
                        bindLinkToCommand(localActions, (int) c.parameters.Last(), c);
                        break;
                    case CommandTypes.End:
                    case CommandTypes.Return:
                        isEnd = jumps.All(x => x.startAddress != _binary.BaseStream.Position);
                        break;
                }
            }

            localActions.ForEach(x => {
                x.data = new Actions(readActions(x.startAddress));
                tryAddLink(actions, x);
            });
            localCalls.ForEach(x => {
                x.data = new AnonymousScriptMethod(readCommands(x.startAddress), x.startAddress);
                tryAddLink(calls, x);
            });
            localJumps.ForEach(x => tryAddLink(jumps, x));
            return commands;
        }

        public Command tryReadCommand() {
            var cmd = tryGetCommandTemplate(_binary.ReadUInt16());
            foreach (var T in cmd.types)
                switch (T.Name) {
                    case "Int32":
                        cmd.parameters.Add(_binary.ReadInt32());
                        if (Command.functionTypes.Contains(cmd.type)) {
                            cmd.parameters[^1] = Convert.ToInt32(_binary.BaseStream.Position + (int) cmd.parameters[^1]);
                        }

                        break;
                    case "UInt16":
                        cmd.parameters.Add(_binary.ReadUInt16());
                        break;
                    case "Byte":
                        cmd.parameters.Add(_binary.ReadByte());
                        break;
                    case "FX32":
                        cmd.parameters.Add(_binary.ReadUInt32() / 0x1000);
                        break;
                    case "FX16":
                        cmd.parameters.Add(_binary.ReadUInt16() / 0x1000);
                        break;
                    default:
                        throw new Exception($"Invalid type \"{T.Name}\".");
                }

            return cmd;
        }

        private void bindLinkToCommand<T>(List<Link<T>> target, int address, Command c) {
            tryAddLink(target, new Link<T> {
                startAddress = address
            });
            c.parameters[^1] = target.Find(x => x.startAddress == address);
        }

        private void tryAddLink<T>(List<Link<T>> target, Link<T> link) {
            if (target.All(x => x.startAddress != link.startAddress)) {
                target.Add(link);
            }
        }

        private Command tryGetCommandTemplate(ushort id) {
            if (!_handler.getCommands().Contains(id))
                throw new Exception($"Unrecognized command ID: {id} @ position {_binary.BaseStream.Position - 0x2}.");
            var def = _handler.getCommand(id);
            return new Command(def.name, def.id, def.type, def.types, def.parameterNames, def.parameterDesc);
        }
    }
}