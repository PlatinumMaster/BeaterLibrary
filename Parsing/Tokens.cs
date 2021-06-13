using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Parsing
{
    public enum TextTokens
    {
        LeftBracket,
        RightBracket,
        LeftBrace,
        RightBrace,
        QuotationMark,
        StringLiteral,
        Comma,
        CompressionFlag,
        ByteSequence,
        TextCharacter,
        ControlCharacter,
        StringTerminator,
        Comment,
    }
}
