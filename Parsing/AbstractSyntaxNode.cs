namespace BeaterLibrary.Parsing
{
    public class AbstractSyntaxNode
    {
        public AbstractSyntaxNode(TextTokens Token, object Value)
        {
            Type = Token;
            this.Value = Value;
        }

        public TextTokens Type { get; }
        public object Value { get; set; }
        public object Attribute { get; set; }
    }
}