namespace BeaterLibrary.Parsing {
    public class AbstractSyntaxNode {
        public AbstractSyntaxNode(TextTokens token, object value) {
            type = token;
            this.value = value;
        }

        public TextTokens type { get; }
        public object value { get; set; }
        public object attribute { get; set; }
    }
}