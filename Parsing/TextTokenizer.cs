using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BeaterLibrary.Parsing
{
    public class TextTokenizer
    {
        int LineNumber { get; set; }

        public TextTokenizer()
        {
            LineNumber = 1;
        }

        private void ThrowException(string reason)
        {
            throw new Exception($"Error on Line {LineNumber}: {reason}");
        }

        public List<List<AbstractSyntaxNode>> Tokenize(string text)
        {
            List<List<AbstractSyntaxNode>> TextArrays = new List<List<AbstractSyntaxNode>>();
            BidirectionalCharEnumerator Input = new BidirectionalCharEnumerator(text);

            if (!Input.HasNext())
                ThrowException("You gotta write something, c'mon.");

            while (Input.HasNext())
            {
                Input.MoveNext();
                switch (Input.Current)
                {
                    case '#':
                        ParseComment(Input);
                        break;
                    case '\r':
                    case '\n':
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            if (!Input.HasNext())
                                ThrowException($"Standalone carriage return character not supported.");
                            Input.MoveNext();
                            if (Input.Current != '\n')
                                ThrowException($"Standalone carriage return character not supported.");
                            LineNumber++;
                        }

                        break;
                    case ' ':
                        break;
                    case '[':
                    case '!':
                        TextArrays.Add(TokenizeArray(Input));
                        break;
                    default:
                        ThrowException($"Invalid character \"{Input.Current}\"");
                        break;
                }
            }

            return TextArrays;
        }

        private List<AbstractSyntaxNode> TokenizeArray(BidirectionalCharEnumerator Input)
        {
            List<AbstractSyntaxNode> Nodes = new List<AbstractSyntaxNode>();
            bool IsCompressed;
            bool EndReached = !Input.HasNext();

            if (EndReached)
                ThrowException("Unexpected end of array.");

            IsCompressed = Input.Current.Equals('!'); // Check if compressed string.

            if (IsCompressed)
            {
                Nodes.Add(new AbstractSyntaxNode(TextTokens.CompressionFlag, '!'));
                Input.MoveNext();
            }

            if (!Input.Current.Equals('['))
                ThrowException("Left bracket is missing.");

            Nodes.Add(new AbstractSyntaxNode(TextTokens.LeftBracket, '['));

            while (!EndReached && Input.MoveNext())
            {
                switch (Input.Current)
                {
                    case '!':
                        ThrowException("Only one compression operator is allowed.");
                        break;
                    case '[':
                        // Beginning of the text array.
                        ThrowException("Only a 1 dimensional array is allowed.");
                        break;
                    case '"':
                        // Beginning of literal.
                        List<AbstractSyntaxNode> QuotationMarkNodes =
                            Nodes.FindAll(x => x.Type == TextTokens.QuotationMark);
                        if (QuotationMarkNodes.Count > 0)
                        {
                            List<AbstractSyntaxNode> OpenQuotationMarkNodes =
                                    QuotationMarkNodes.FindAll(x => x.Attribute.ToString().Equals("Open")),
                                CloseQuotationMarkNodes =
                                    QuotationMarkNodes.FindAll(x => x.Attribute.ToString().Equals("Close"));

                            if (OpenQuotationMarkNodes.Count != CloseQuotationMarkNodes.Count)
                                ThrowException(
                                    "One or more strings in the array were not terminated correctly with a quotation mark.");
                            else if (Nodes[Nodes.Count - 1].Type != TextTokens.Comma)
                                ThrowException("Strings containing multiple lines must be separated by a comma.");
                        }

                        Nodes.Add(new AbstractSyntaxNode(TextTokens.QuotationMark, '"') {Attribute = "Open"});
                        foreach (AbstractSyntaxNode Node in ParseLiteral(Input))
                            Nodes.Add(Node);
                        break;
                    case ']':
                        // End of the text array.
                        if (Nodes[Nodes.Count - 1].Type != TextTokens.QuotationMark)
                            ThrowException("String was not terminated before using ']'.");

                        AbstractSyntaxNode Last = Nodes.FindLast(x => x.Type == TextTokens.StringTerminator);
                        if (Last == null)
                            ThrowException(
                                "You forgot to terminate the last string in the array with '$'. Please do so, or else your text will not save correctly.");

                        Nodes.Add(new AbstractSyntaxNode(TextTokens.RightBracket, Input.Current));
                        EndReached = !EndReached;
                        break;
                    case ',':
                        // Comma, denoting the next literal.
                        if (Nodes[Nodes.Count - 1].Type != TextTokens.QuotationMark)
                            ThrowException("A comma should only be placed after a quotation mark.");
                        Nodes.Add(new AbstractSyntaxNode(TextTokens.Comma, Input.Current));
                        break;
                    default:
                        // Never seen this character in my life.
                        if (!char.IsWhiteSpace(Input.Current))
                            ThrowException($"Unrecognized character \"{Input.Current}\".");
                        if (Input.Current == '\r')
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                if (!Input.HasNext())
                                    ThrowException($"Standalone carriage return character not supported.");
                                Input.MoveNext();
                                if (Input.Current != '\n')
                                    ThrowException($"Standalone carriage return character not supported.");
                                LineNumber++;
                            }

                        break;
                }
            }

            if (!EndReached)
                ThrowException("Missing string array terminator.");
            return Nodes;
        }

        private void ParseComment(BidirectionalCharEnumerator Input)
        {
            bool EndReached = !Input.HasNext();
            while (!EndReached && Input.MoveNext())
            {
                switch (Input.Current)
                {
                    case '\r':
                    case '\n':
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            if (!Input.HasNext())
                                ThrowException("Unsupported: Carriage return character, but no newline character.");
                            Input.MoveNext();
                            if (Input.Current != '\n')
                                ThrowException("Unsupported: Carriage return character, but no newline character.");
                            LineNumber++;
                        }

                        EndReached = !EndReached;
                        break;
                    default:
                        break;
                }
            }
        }

        private AbstractSyntaxNode ParseByteSequence(BidirectionalCharEnumerator Input)
        {
            StringBuilder ByteSequence = new StringBuilder();
            int ByteSequenceParsed = 0;

            if (!Input.HasNext())
                ThrowException(
                    "\"\\x\" is a reserved directive. It works fine without it being escaped. Remove the escape operator and try again.");

            while (ByteSequenceParsed < 4 && Input.MoveNext())
            {
                if (char.IsLetter(Input.Current))
                    if (char.ToLower(Input.Current) < 97 || char.ToLower(Input.Current) > 102)
                        ThrowException("Not a valid hexadecimal character!");
                    else if (char.IsDigit(Input.Current))
                        if (Input.Current < 48 || Input.Current > 57)
                            ThrowException("Not a valid hexadecimal character!");
                ByteSequence.Append(Input.Current);
                ByteSequenceParsed++;
            }

            if (ByteSequenceParsed < 4)
                ThrowException("Unexpected end of file.");

            return new AbstractSyntaxNode(TextTokens.ByteSequence,
                ushort.Parse(ByteSequence.ToString(), System.Globalization.NumberStyles.HexNumber));
        }

        private List<AbstractSyntaxNode> ParseLiteral(BidirectionalCharEnumerator Input)
        {
            List<AbstractSyntaxNode> TokenizedLiteral = new List<AbstractSyntaxNode>();
            bool EndReached = !Input.HasNext();

            if (EndReached)
                ThrowException("Invalid literal.");

            while (!EndReached && Input.MoveNext())
            {
                switch (Input.Current)
                {
                    case '\\':
                        if (!Input.HasNext())
                            ThrowException("Ending quotation mark is missing.");
                        Input.MoveNext();
                        // Check for control characters.
                        switch (Input.Current)
                        {
                            case 'c':
                                // Macro for text clearing.
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xF000));
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xBE01));
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0x0));
                                break;
                            case 'n':
                                // Macro for new line.
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xFFFE));
                                break;
                            case 'l':
                                // Macro for line scroll.
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xF000));
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xBE00));
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0x0));
                                break;
                            case 'x':
                                // Macro for a byte sequence.
                                TokenizedLiteral.Add(ParseByteSequence(Input));
                                break;
                            default:
                                // Escaped character.
                                TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.TextCharacter, Input.Current));
                                break;
                        }

                        break;
                    case '{':
                        // String variable.
                        break;
                    case '"':
                        // Delimiter for literal.
                        TokenizedLiteral.Add(
                            new AbstractSyntaxNode(TextTokens.QuotationMark, '"') {Attribute = "Close"});
                        EndReached = !EndReached;
                        break;
                    case '$':
                        TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.StringTerminator, 0xFFFF));
                        break;
                    default:
                        TokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.TextCharacter, Input.Current));
                        break;
                }
            }

            if (!EndReached)
                ThrowException("Missing string terminator.");

            return TokenizedLiteral;
        }
    }
}