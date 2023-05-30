using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BeaterLibrary.Formats.Scripts {
    public class CommandsListHandler {
        private readonly Dictionary<string, ushort> _commandMap;
        private readonly Dictionary<ushort, Command> _commands;
        public CommandsListHandler(string game, string configurationPath) {
            _commands = new Dictionary<ushort, Command>();
            _commandMap = new Dictionary<string, ushort>();
            AddCommandsFromYaml(Path.Combine(configurationPath, game, "Base.yml"));
        }
        
        public CommandsListHandler(string game, string configurationPath, int plugins) : this(game, configurationPath) {
            if (plugins != -1) AddCommandsFromYaml(Path.Combine(configurationPath, game, $"Overlay {plugins}.yml"));
        }
        
        private void AddCommandsFromYaml(string yamlPath) {
            var s = File.OpenText(yamlPath);
            var Deserializer = new Deserializer();
            var CommandsYaml = Deserializer.Deserialize<Dictionary<int, YamlMappingNode>>(s);
            foreach (var KeyValuePair in CommandsYaml) {
                int Num;
                YamlMappingNode YamlMappingNode;
                KeyValuePair.Deconstruct(out Num, out YamlMappingNode);
                var Key = Num;
                var Node = YamlMappingNode;
                var Cmd = ReadCommandDetail(Node, Key);
                _commands.Add((ushort) Key, Cmd);
                _commandMap.Add(Cmd.Name, (ushort) Key);
            }
        }
        
        private static Command ReadCommandDetail(YamlMappingNode node, int key) {
            List<Type> Types;
            CommandTypes CmdType;
            List<string> ParameterNames;
            List<string> ParameterDesc;
            ReadCommandParameters(node, out Types, out CmdType, out ParameterNames, out ParameterDesc);
            return new Command(node["Name"].ToString(), (ushort) key, CmdType, Types, ParameterNames, ParameterDesc);
        }

        public Command GetCommand(ushort id) {
            return _commands[id];
        }

        public Dictionary<ushort, Command>.KeyCollection GetCommands() {
            return _commands.Keys;
        }

        private static void ReadCommandParameters(YamlMappingNode node, out List<Type> types, out CommandTypes cmdType,
            out List<string> parameterNames, out List<string> parameterDesc) {
            types = new List<Type>();
            parameterNames = new List<string>();
            parameterDesc = new List<string>();
            cmdType = CommandTypes.Default;
            var IsExternCall = false;
            if (node.Children.ContainsKey("ExternCall")) bool.TryParse(node["ExternCall"].ToString(), out IsExternCall);
            if (node.Children.ContainsKey("Parameters")) {
                var ParameterIndex = 0;
                foreach (YamlMappingNode KeyValuePair in (node["Parameters"] as YamlSequenceNode).Children) {
                    parameterNames.Add(KeyValuePair.Children.ContainsKey("Name")
                        ? KeyValuePair["Name"].ToString()
                        : $"unk{ParameterIndex}");
                    switch (KeyValuePair["Type"].ToString().ToLower()) {
                        case "int":
                            types.Add(typeof(int));
                            break;
                        case "returnable int":
                        case "returnable bool":
                        case "const ushort":
                        case "ref ushort":
                        case "ushort":
                            types.Add(typeof(ushort));
                            break;
                        case "byte":
                            types.Add(typeof(byte));
                            break;
                        case "fx32":
                            types.Add(typeof(FX32));
                            break;
                        case "fx16":
                            types.Add(typeof(FX16));
                            break;
                    }

                    ParameterIndex++;
                }
            }

            if (node.Children.ContainsKey("CommandType")) {
                cmdType = CommandTypeLookup.SpecialCommandTypeLUT[((YamlScalarNode) node["CommandType"]).Value];
                if (cmdType == CommandTypes.Call && IsExternCall) cmdType = CommandTypes.Default;
            }
        }
    }
}