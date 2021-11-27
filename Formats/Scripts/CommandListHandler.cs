using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace BeaterLibrary.Formats.Scripts
{
    public class CommandsListHandler
    {
        private readonly Dictionary<string, ushort> command_map;
        private readonly Dictionary<ushort, Command> Commands;

        public CommandsListHandler(string game, string ConfigurationPath)
        {
            Commands = new Dictionary<ushort, Command>();
            command_map = new Dictionary<string, ushort>();
            // Parse Commands from YAML, and store them.
            using var s = File.OpenText(Path.Combine(ConfigurationPath, $"{game}.yml"));
            var deserializer = new Deserializer();
            var commands_yaml = deserializer.Deserialize<Dictionary<int, YamlMappingNode>>(s);

            foreach (var (key, node) in commands_yaml)
            {
                var cmd = ReadCommandDetail(node, key);
                Commands.Add((ushort) key, cmd);
                command_map.Add(cmd.Name, (ushort) key);
            }
        }

        public CommandsListHandler(string game, string ConfigurationPath, int[] plugins) : this(game,
            ConfigurationPath)
        {
            if (plugins is {Length: > 0})
            {
                using var s = File.OpenText(Path.Combine(ConfigurationPath, "OverlayPlugins", game,
                    $"ovl{string.Join("_", plugins)}.yml"));
                var deserializer = new Deserializer();
                var commands_yaml = deserializer.Deserialize<Dictionary<int, YamlMappingNode>>(s);

                foreach (var (key, node) in commands_yaml)
                {
                    var cmd = ReadCommandDetail(node, key);
                    Commands.Add((ushort) key, cmd);
                    command_map.Add(cmd.Name, (ushort) key);
                }
            }
        }

        private static Command ReadCommandDetail(YamlMappingNode node, int key)
        {
            return new(node["Name"].ToString(), (ushort) key, GetCommandType(node), ReadCommandParameters(node));
        }

        public Command GetCommand(ushort id)
        {
            return Commands[id];
        }

        public Dictionary<ushort, Command>.KeyCollection GetCommands()
        {
            return Commands.Keys;
        }

        private static List<Type> ReadCommandParameters(YamlMappingNode Node)
        {
            var types = new List<Type>();
            if (Node.Children.ContainsKey("Parameters"))
            {
                var parameters = (YamlSequenceNode) Node["Parameters"];
                foreach (var p in parameters.Children)
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

            return types;
        }

        private static CommandTypes GetCommandType(YamlMappingNode Node)
        {
            if (!Node.Children.ContainsKey("CommandType"))
                return CommandTypes.Default;

            switch (Node["CommandType"].ToString())
            {
                case "Jump":
                    return CommandTypes.Jump;
                case "ConditionalJump":
                    return CommandTypes.ConditionalJump;
                case "Call":
                    return CommandTypes.Call;
                case "End":
                    return CommandTypes.End;
                case "Return":
                    return CommandTypes.Return;
                case "Movement":
                    return CommandTypes.Actions;
                default:
                    return CommandTypes.Default;
            }
        }
    }
}