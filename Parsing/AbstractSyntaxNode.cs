using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Parsing
{
    public class AbstractSyntaxNode
    {
        public TextTokens Type { get; }
        public object Value { get; set; }
        public object Attribute { get; set; }
        public AbstractSyntaxNode(TextTokens Token, object Value)
        {
            Type = Token;
            this.Value = Value;
        }

    }
}
