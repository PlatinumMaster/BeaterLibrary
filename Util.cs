using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BeaterLibrary.Formats.Scripts;

namespace BeaterLibrary {
    public static class Util {
        private static List<int> ParsedJumpOffsets = new List<int>();
        public static bool isNumericType(object parameter) {
            return parameter is sbyte || parameter is byte
                                      || parameter is short || parameter is ushort
                                      || parameter is int || parameter is uint;
        }

        public static string unpackScriptContainer(ScriptContainer sc) {
            var s = new StringBuilder();
            var jumpOffsets = sc.jumps.Select(x => x.startAddress).ToList();

            ParsedJumpOffsets = new List<int>();
            foreach (var script in sc.scripts) {
                unpackMethod(script, $"Script_{sc.scripts.IndexOf(script) + 1}", s, jumpOffsets);
            }

            foreach (var function in sc.calls) {
                unpackMethod(function.data, function.ToString(), s, jumpOffsets);
            }

            foreach (var actions in sc.actions) s.Append($"ActionSequence {actions.getDataToString()}\n");

            return s.ToString();
        }

        private static void unpackMethod(ScriptMethod script, string scriptName, StringBuilder s,
            List<int> ScrJumpOffsets) {
            var baseAddress = script.address;
            s.Append($"{scriptName}:\n");
            foreach (var c in script.commands) {
                bool AlreadyParsed = ParsedJumpOffsets.Contains(baseAddress);
                if (ScrJumpOffsets.Contains(baseAddress) && !AlreadyParsed) {
                    s.Append($"AnonymousScriptMethod_{baseAddress}:");
                    ParsedJumpOffsets.Add(baseAddress);
                    s.AppendLine();
                }

                baseAddress += c.size();
                
                if (ParsedJumpOffsets.Contains(baseAddress)) {
                    continue;
                }

                s.Append($"\t{c}\n");
            }

            s.Append('\n');
        }

        public static void generateCommandAsm(string game, string configurationPath, int scriptPlugins) {
            var cmd = new CommandsListHandler(game, configurationPath, scriptPlugins);
            using var o = new StreamWriter($"{game}.s");
            // Helper Macros
            o.WriteLine("@ Helper Macros");

            // Script: For the purpose of declaring each script in the header.
            o.WriteLine(
                @".macro script, address
.word  \address - . - 4
.endm");
            o.WriteLine();

            // EndHeader: Declares the end of the header section.
            o.WriteLine(
                @".macro EndHeader
.hword 0xFD13
.endm");
            o.WriteLine();

            // Function: List of Actions.
            o.WriteLine(
                @".macro Function label
.align 2
\label:
.endm");
            // ActionSequence: List of Actions.
            o.WriteLine(
                @".macro ActionSequence label
.align 2
\label:
.endm");
            o.WriteLine();

            // Movement: Declares a new movement instruction.
            o.WriteLine(
                @".macro Action x y
.hword \x
.hword \y
.endm");
            o.WriteLine();

            // EndMovement: Declares the end of a movement.
            o.WriteLine(
                @".macro TerminateActionSequence
.hword 0xFE
.hword 0x00
.endm");
            o.WriteLine();

            // Write all of the commands from the YAML.
            o.WriteLine("@ -----------------");
            o.WriteLine("@ Script Commands");
            foreach (var key in cmd.getCommands()) {
                var c = cmd.getCommand(key);
                o.Write($".macro {c.name} ");
                for (var i = 0; i < c.types.Count; i++)
                    o.Write($"p{i}{(i == c.types.Count - 1 ? "" : ",")} ");
                o.WriteLine();
                o.WriteLine($".hword {c.id}");

                var j = 0;
                foreach (var type in c.types)
                    switch (type.Name) {
                        case "Int32":
                            var isBranch = c.type is CommandTypes.Call || c.type is CommandTypes.ActionSequence ||
                                           c.type is CommandTypes.ConditionalJump || c.type is CommandTypes.Jump;
                            o.WriteLine(isBranch ? $".word (\\p{j++} - .) - 4" : $".word \\p{j++}");
                            break;
                        case "UInt16":
                            o.WriteLine($".hword \\p{j++}");
                            break;
                        case "Byte":
                            o.WriteLine($".byte \\p{j++}");
                            break;
                        case "FX32":
                            o.WriteLine($".word \\p{j++} * 4096");
                            break;
                        case "FX16":
                            o.WriteLine($".hword \\p{j++} * 4096");
                            break;
                    }

                o.WriteLine(
                    @".endm
                    ");
            }
        }
    }
}