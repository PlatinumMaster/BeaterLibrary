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
            addCommandsFromYaml(Path.Combine(configurationPath, game, "Base.yml"));
        }
        
        public CommandsListHandler(string game, string configurationPath, int plugins) : this(game, configurationPath) {
            if (plugins != -1) addCommandsFromYaml(Path.Combine(configurationPath, game, $"Overlay {plugins}.yml"));
        }
        
        private void addCommandsFromYaml(string yamlPath) {
            var s = File.OpenText(yamlPath);
            var deserializer = new Deserializer();
            var commandsYaml = deserializer.Deserialize<Dictionary<int, YamlMappingNode>>(s);
            foreach (var keyValuePair in commandsYaml) {
                int num;
                YamlMappingNode yamlMappingNode;
                keyValuePair.Deconstruct(out num, out yamlMappingNode);
                var key = num;
                var node = yamlMappingNode;
                var cmd = readCommandDetail(node, key);
                _commands.Add((ushort) key, cmd);
                _commandMap.Add(cmd.name, (ushort) key);
            }
        }
        
        private static Command readCommandDetail(YamlMappingNode node, int key) {
            List<Type> types;
            CommandTypes cmdType;
            List<string> parameterNames;
            List<string> parameterDesc;
            readCommandParameters(node, out types, out cmdType, out parameterNames, out parameterDesc);
            return new Command(node["Name"].ToString(), (ushort) key, cmdType, types, parameterNames, parameterDesc);
        }

        public Command getCommand(ushort id) {
            return _commands[id];
        }

        public Dictionary<ushort, Command>.KeyCollection getCommands() {
            return _commands.Keys;
        }

        private static void readCommandParameters(YamlMappingNode node, out List<Type> types, out CommandTypes cmdType,
            out List<string> parameterNames, out List<string> parameterDesc) {
            types = new List<Type>();
            parameterNames = new List<string>();
            parameterDesc = new List<string>();
            cmdType = CommandTypes.Default;
            var isExternCall = false;
            if (node.Children.ContainsKey("ExternCall")) bool.TryParse(node["ExternCall"].ToString(), out isExternCall);
            if (node.Children.ContainsKey("Parameters")) {
                var parameterIndex = 0;
                foreach (YamlMappingNode keyValuePair in (node["Parameters"] as YamlSequenceNode).Children) {
                    parameterNames.Add(keyValuePair.Children.ContainsKey("Name")
                        ? keyValuePair["Name"].ToString()
                        : $"unk{parameterIndex}");
                    switch (keyValuePair["Type"].ToString().ToLower()) {
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

                    parameterIndex++;
                }
            }

            if (node.Children.ContainsKey("CommandType")) {
                cmdType = CommandTypeLookup.SpecialCommandTypeLUT[((YamlScalarNode) node["CommandType"]).Value];
                if (cmdType == CommandTypes.Call && isExternCall) cmdType = CommandTypes.Default;
            }
        }
    }
}