using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BeaterLibrary.Formats.Scripts;

namespace BeaterLibrary {
    public static class Util {
        public static bool isNumericType(object parameter) {
            return parameter is sbyte || parameter is byte
                                      || parameter is short || parameter is ushort
                                      || parameter is int || parameter is uint;
        }

        public static string unpackScriptContainer(ScriptContainer sc) {
            var s = new StringBuilder();
            var jumpOffsets = sc.Jumps.Select(x => x.StartAddress).ToList();
            foreach (var script in sc.Scripts)
                unpackMethod(script, $"Script_{sc.Scripts.IndexOf(script) + 1}", s, jumpOffsets);

            foreach (var function in sc.Calls) unpackMethod(function.Data, function.ToString(), s, jumpOffsets);

            foreach (var actions in sc.Actions) s.Append($"ActionSequence {actions.GetDataToString()}\n");

            return s.ToString();
        }

        private static void unpackMethod(ScriptMethod script, string scriptName, StringBuilder s,
            List<int> jumpOffsets) {
            var baseAddress = script.Address;
            s.Append($"{scriptName}:\n");
            foreach (var c in script.Commands) {
                if (jumpOffsets.Contains(baseAddress)) {
                    s.Append($"AnonymousScriptMethod_{baseAddress}:");
                    s.AppendLine();
                }

                s.Append($"\t{c}\n");
                baseAddress += c.Size();
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
            foreach (var key in cmd.GetCommands()) {
                var c = cmd.GetCommand(key);
                o.Write($".macro {c.Name} ");
                for (var i = 0; i < c.Types.Count; i++)
                    o.Write($"p{i}{(i == c.Types.Count - 1 ? "" : ",")} ");
                o.WriteLine();
                o.WriteLine($".hword {c.ID}");

                var j = 0;
                foreach (var type in c.Types)
                    switch (type.Name) {
                        case "Int32":
                            var isBranch = c.Type is CommandTypes.Call || c.Type is CommandTypes.ActionSequence ||
                                           c.Type is CommandTypes.ConditionalJump || c.Type is CommandTypes.Jump;
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