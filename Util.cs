using BeaterLibrary.Formats.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BeaterLibrary
{
    public static class Util
    {
        public static bool IsNumericType(object Parameter)
        {
            return Parameter is sbyte || Parameter is byte
                  || Parameter is short || Parameter is ushort
                  || Parameter is int || Parameter is uint;
        }
        public static string UnpackScriptContainer(ScriptContainer SC)
        {
            StringBuilder S = new StringBuilder();
            List<int> JumpOffsets = SC.Jumps.Select(x => x.StartAddress).ToList();
            foreach (ScriptMethod Script in SC.Scripts)
                UnpackMethod(Script, $"Script_{SC.Scripts.IndexOf(Script)}", S, JumpOffsets);
            foreach (Link<AnonymousScriptMethod> Function in SC.Calls)
                UnpackMethod(Function.Data, Function.ToString(), S, JumpOffsets);
            foreach (Link<Actions> Actions in SC.Actions)
                S.Append($"ActionSequence {Actions.GetDataToString()}\n");
            return S.ToString();
        }

        private static void UnpackMethod(ScriptMethod Script, string ScriptName, StringBuilder S, List<int> JumpOffsets)
        {
            int BaseAddress = Script.Address;
            S.Append($"{ScriptName}:\n");
            foreach (Command C in Script.Commands)
            {
                if (JumpOffsets.Contains(BaseAddress))
                {
                    S.Append($"AnonymousScriptMethod_{BaseAddress}:");
                    S.AppendLine();
                }
                
                S.Append($"\t{C}\n");
                BaseAddress += C.Size();
            }
            S.Append('\n');
        }

        public static void GenerateCommandASM(string game, int[][] ScriptPlugins)
        {
            CommandsListHandler cmd = new CommandsListHandler(game, ScriptPlugins);
            using StreamWriter o = new StreamWriter($"{game}.s");
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
            foreach (ushort key in cmd.GetCommands())
            {
                Command c = cmd.GetCommand(key);
                o.Write($".macro {c.Name} ");
                for (int i = 0; i < c.Types.Count; i++)
                    o.Write($"p{i}{(i == c.Types.Count - 1 ? "" : ",")} ");
                o.WriteLine();
                o.WriteLine($".hword {c.ID}");

                int j = 0;
                foreach (var type in c.Types)
                {
                    switch (type.Name)
                    {
                        case "Int32":
                            bool IsBranch = c.Type is CommandTypes.Call || c.Type is CommandTypes.Actions ||
                                            c.Type is CommandTypes.ConditionalJump || c.Type is CommandTypes.Jump;
                            o.WriteLine(IsBranch ? $".word (\\p{j++} - .) - 4" : $".word \\p{j++}");
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
                }

                o.WriteLine(
                    @".endm
                    ");
            }
        }
    }
}