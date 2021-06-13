using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BeaterLibrary.Formats.Scripts
{
    class FX32 {}
    class FX16 {}
    public class CommandsListHandler
    {
        private Dictionary<ushort, Command> commands;
        private Dictionary<string, ushort> command_map;

        public CommandsListHandler(string game)
        {
            commands = new Dictionary<ushort, Command>();
            command_map = new Dictionary<string, ushort>();
            // Parse Commands from YAML, and store them.
            using var s = File.OpenText($"{game}.yml");
            var deserializer = new Deserializer();
            var commands_yaml = deserializer.Deserialize<Dictionary<int, YamlMappingNode>>(s);

            foreach (var (key, node) in commands_yaml)
            {
                var cmd = ReadCommandDetail(node, key);
                commands.Add((ushort)key, cmd);
                command_map.Add(cmd.Name, (ushort)key);
            }
        }

        private static Command ReadCommandDetail(YamlMappingNode node, int key)
        {
            var name = node["Name"].ToString();
            var types = ReadCommandParameters(node);

            bool hasFunction = node.Children.ContainsKey("HasFunction") && node["HasFunction"].ToString() == "true",
            hasMovement = node.Children.ContainsKey("HasMovement") && node["HasMovement"].ToString() == "true",
            isEnd = node.Children.ContainsKey("IsEnd") && node["IsEnd"].ToString() == "true",
            dynamicParams = node.Children.ContainsKey("DynamicParameters") && node["DynamicParameters"].ToString() == "true";

            return new Command(name, (ushort)key, hasFunction, hasMovement, isEnd, dynamicParams, types);
        }

        public Command GetCommand(ushort id)
        {
            return commands[id];
        }

        public Dictionary<ushort, Command>.KeyCollection GetCommands()
        {
            return commands.Keys;
        }

        private static List<Type> ReadCommandParameters(YamlMappingNode node)
        {
            List<Type> types = new List<Type>();
            if (node.Children.ContainsKey("Parameters"))
            {
                YamlSequenceNode parameters = (YamlSequenceNode)node["Parameters"];
                foreach (var p in parameters.Children)
                {
                    switch (p.ToString())
                    {
                        case "int":
                            types.Add(typeof(int));
                            break;
                        case "const ushort":
                        case "ref ushort":
                        case "ushort":
                            types.Add(typeof(ushort));
                            break;
                        case "byte":
                            types.Add(typeof(byte));
                            break;
                        case "FX32":
                            types.Add(typeof(FX32));
                            break;
                        case "FX16":
                            types.Add(typeof(FX16));
                            break;
                    }
                }
            }


            return types;
        }
    }
}
