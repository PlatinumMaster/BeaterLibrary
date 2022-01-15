namespace BeaterLibrary.Formats.Scripts {
    public class Link<T> {
        public int startAddress { get; set; }
        public T data { get; set; }

        public override string ToString() => $"{typeof(T).Name}_{startAddress}";
        public string getDataToString() => $"{typeof(T).Name}_{startAddress}\n{data}";
    }
}