using BeaterLibrary.Formats.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeaterLibrary
{
    public static class Util
    {
        public static string Flatten<T>(List<List<T>> l, string type, bool isScript)
        {
            int counter = 0;
            string output = "";
            foreach (List<T> li in l)
            {
                StringBuilder st = new StringBuilder();
                for (int i = 0; i < li.Count(); ++i)
                    st.Append($"\t{li[i]}{Environment.NewLine}");

                output = string.Join(Environment.NewLine, output, $"{type}{counter++}{(isScript ? ":" : "")}{Environment.NewLine}{st}{Environment.NewLine}");
            }
            return output;
        }

        public static void GenerateCommandASM(string game)
        {
            CommandsListHandler cmd = new CommandsListHandler(game);
            using System.IO.StreamWriter o = new System.IO.StreamWriter($"{game}.s");
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

            // Movement: Declares a new movement instruction.
            o.WriteLine(
            @".macro Movement x y
.hword \x
.hword \y
.endm");
            o.WriteLine();

            // MovementLabel: Declares a a new movement label. Needed for padding.
            o.WriteLine(
            @".macro MovementLabel label
.align 4
\label:
.endm");
            o.WriteLine();

            // FunctionLabel: Declares a a new function label. Needed for padding.
            o.WriteLine(
            @".macro FunctionLabel label
.align 4
\label:
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
                            if (c.HasFunction || c.HasMovement)
                            {
                                o.WriteLine($".word (\\p{j++} - .) - 4");
                                break;
                            }
                            o.WriteLine($".word \\p{j++}");
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