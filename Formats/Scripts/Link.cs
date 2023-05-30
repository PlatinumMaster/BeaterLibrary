namespace BeaterLibrary.Formats.Scripts {
    public class Link<T> {
        public int StartAddress { get; set; }
        public T Data { get; set; }

        public override string ToString() => $"{typeof(T).Name}_{StartAddress}";
        public string GetDataToString() => $"{typeof(T).Name}_{StartAddress}\n{Data}";
    }
}