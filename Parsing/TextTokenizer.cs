using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BeaterLibrary.Parsing {
    public class TextTokenizer {
        public TextTokenizer() {
            lineNumber = 1;
        }

        private int lineNumber { get; set; }

        private void throwException(string reason) {
            throw new Exception($"Error on Line {lineNumber}: {reason}");
        }

        public List<List<AbstractSyntaxNode>> tokenize(string text) {
            var textArrays = new List<List<AbstractSyntaxNode>>();
            var input = new BidirectionalCharEnumerator(text);

            if (!input.hasNext())
                throwException("You gotta write something, c'mon.");

            while (input.hasNext()) {
                input.moveNext();
                switch (input.current) {
                    case '#':
                        parseComment(input);
                        break;
                    case '\r':
                        if (Environment.OSVersion.Platform != PlatformID.Unix) {
                            if (!input.hasNext())
                                throwException("Standalone carriage return character not supported.");
                            input.moveNext();
                            if (input.current != '\n')
                                throwException("Standalone carriage return character not supported.");
                        }

                        lineNumber++;
                        break;
                    case '\n':
                        lineNumber++;
                        break;
                    case ' ':
                        break;
                    case '[':
                    case '!':
                        textArrays.Add(tokenizeArray(input));
                        break;
                    default:
                        throwException($"Invalid character \"{input.current}\"");
                        break;
                }
            }

            return textArrays;
        }

        private List<AbstractSyntaxNode> tokenizeArray(BidirectionalCharEnumerator input) {
            var nodes = new List<AbstractSyntaxNode>();
            bool isCompressed;
            var endReached = !input.hasNext();

            if (endReached)
                throwException("Unexpected end of array.");

            isCompressed = input.current.Equals('!'); // Check if compressed string.

            if (isCompressed) {
                nodes.Add(new AbstractSyntaxNode(TextTokens.CompressionFlag, '!'));
                input.moveNext();
            }

            if (!input.current.Equals('['))
                throwException("Left bracket is missing.");

            nodes.Add(new AbstractSyntaxNode(TextTokens.LeftBracket, '['));

            while (!endReached && input.moveNext())
                switch (input.current) {
                    case '!':
                        throwException("Only one compression operator is allowed.");
                        break;
                    case '[':
                        // Beginning of the text array.
                        throwException("Only a 1 dimensional array is allowed.");
                        break;
                    case '"':
                        // Beginning of literal.
                        var quotationMarkNodes =
                            nodes.FindAll(x => x.type == TextTokens.QuotationMark);
                        if (quotationMarkNodes.Count > 0) {
                            List<AbstractSyntaxNode> openQuotationMarkNodes =
                                    quotationMarkNodes.FindAll(x => x.attribute.ToString().Equals("Open")),
                                closeQuotationMarkNodes =
                                    quotationMarkNodes.FindAll(x => x.attribute.ToString().Equals("Close"));

                            if (openQuotationMarkNodes.Count != closeQuotationMarkNodes.Count)
                                throwException(
                                    "One or more strings in the array were not terminated correctly with a quotation mark.");
                            else if ((char) nodes[^1].value != 0xFFFE)
                                throwException("Strings containing multiple lines must be separated by a comma.");
                        }

                        nodes.Add(new AbstractSyntaxNode(TextTokens.QuotationMark, '"') {attribute = "Open"});
                        foreach (var node in parseLiteral(input))
                            nodes.Add(node);
                        break;
                    case ']':
                        // End of the text array.
                        if (nodes[nodes.Count - 1].type != TextTokens.QuotationMark)
                            throwException("String was not terminated before using ']'.");

                        nodes.Add(new AbstractSyntaxNode(TextTokens.RightBracket, input.current));
                        endReached = !endReached;
                        break;
                    case ',':
                        // Comma, denoting the next literal.
                        if (nodes[nodes.Count - 1].type != TextTokens.QuotationMark)
                            throwException("A comma should only be placed after a quotation mark.");
                        nodes.Add(new AbstractSyntaxNode(TextTokens.ControlCharacter, (char)0xFFFE));
                        break;
                    default:
                        // Never seen this character in my life.
                        if (!char.IsWhiteSpace(input.current))
                            throwException($"Unrecognized character \"{input.current}\".");
                        if (input.current == '\r')
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                                if (!input.hasNext())
                                    throwException("Standalone carriage return character not supported.");
                                input.moveNext();
                                if (input.current != '\n')
                                    throwException("Standalone carriage return character not supported.");
                                lineNumber++;
                            }

                        break;
                }

            if (!endReached)
                throwException("Missing string array terminator.");
            return nodes;
        }

        private void parseComment(BidirectionalCharEnumerator input) {
            var endReached = !input.hasNext();
            while (!endReached && input.moveNext())
                switch (input.current) {
                    case '\r':
                    case '\n':
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                            if (!input.hasNext())
                                throwException("Unsupported: Carriage return character, but no newline character.");
                            input.moveNext();
                            if (input.current != '\n')
                                throwException("Unsupported: Carriage return character, but no newline character.");
                            lineNumber++;
                        }

                        endReached = !endReached;
                        break;
                }
        }

        private AbstractSyntaxNode parseByteSequence(BidirectionalCharEnumerator input) {
            var byteSequence = new StringBuilder();
            var byteSequenceParsed = 0;

            if (!input.hasNext())
                throwException(
                    "\"\\x\" is a reserved directive. It works fine without it being escaped. Remove the escape operator and try again.");

            while (byteSequenceParsed < 4 && input.moveNext()) {
                if (char.IsLetter(input.current))
                    if (char.ToLower(input.current) < 97 || char.ToLower(input.current) > 102)
                        throwException("Not a valid hexadecimal character!");
                    else if (char.IsDigit(input.current))
                        if (input.current < 48 || input.current > 57)
                            throwException("Not a valid hexadecimal character!");
                byteSequence.Append(input.current);
                byteSequenceParsed++;
            }

            if (byteSequenceParsed < 4)
                throwException("Unexpected end of file.");

            return new AbstractSyntaxNode(TextTokens.ByteSequence,
                ushort.Parse(byteSequence.ToString(), NumberStyles.HexNumber));
        }

        private List<AbstractSyntaxNode> parseLiteral(BidirectionalCharEnumerator input) {
            var tokenizedLiteral = new List<AbstractSyntaxNode>();
            var endReached = !input.hasNext();

            if (endReached)
                throwException("Invalid literal.");

            while (!endReached && input.moveNext())
                switch (input.current) {
                    case '\\':
                        if (!input.hasNext())
                            throwException("Ending quotation mark is missing.");
                        input.moveNext();
                        // Check for control characters.
                        switch (input.current) {
                            case 'c':
                                // Macro for text clearing.
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xF000));
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xBE01));
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0x0));
                                break;
                            case 'n':
                                // Macro for new line.
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xFFFE));
                                break;
                            case 'l':
                                // Macro for line scroll.
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xF000));
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0xBE00));
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.ByteSequence, 0x0));
                                break;
                            case 'x':
                                // Macro for a byte sequence.
                                tokenizedLiteral.Add(parseByteSequence(input));
                                break;
                            default:
                                // Escaped character.
                                tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.TextCharacter, input.current));
                                break;
                        }

                        break;
                    case '{':
                        // String variable.
                        break;
                    case '"':
                        // Delimiter for literal.
                        tokenizedLiteral.Add(
                            new AbstractSyntaxNode(TextTokens.QuotationMark, '"') {attribute = "Close"});
                        endReached = true;
                        break;
                    case '$':
                        tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.StringTerminator, (char)0xFFFF));
                        break;
                    default:
                        tokenizedLiteral.Add(new AbstractSyntaxNode(TextTokens.TextCharacter, input.current));
                        break;
                }

            if (!endReached)
                throwException("Missing string terminator.");

            return tokenizedLiteral;
        }
    }
}