using System.Collections.Generic;

namespace BeaterLibrary.Formats.Scripts {
    public static class CommandTypeLookup {
        public static Dictionary<string, CommandTypes> SpecialCommandTypeLUT = new() {
            {
                "Jump",
                CommandTypes.Jump
            }, {
                "Return",
                CommandTypes.Return
            }, {
                "End",
                CommandTypes.End
            }, {
                "Call",
                CommandTypes.Call
            }, {
                "CallActionSeq",
                CommandTypes.ActionSequence
            }
        };
    }
}