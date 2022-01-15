namespace BeaterLibrary.Parsing {
    public class BidirectionalCharEnumerator {
        private readonly string _data;
        private int _index = -1;

        public BidirectionalCharEnumerator(string data) {
            _data = data;
        }

        public char current => _index >= 0 && _index < _data.Length ? _data[_index] : (char) 0;

        public bool moveNext() {
            return ++_index >= 0 && _index < _data.Length;
        }

        public bool moveBack() {
            return --_index >= 0 && _index < _data.Length;
        }

        public bool hasNext() {
            return _index + 1 >= 0 && _index + 1 < _data.Length;
        }

        public bool hasPrev() {
            return _index - 1 >= 0 && _index - 1 < _data.Length;
        }

        public void reset() {
            _index = -1;
        }

        public char peekNext() {
            if (hasNext()) {
                char data;
                moveNext();
                data = current;
                moveBack();
                return data;
            }

            return (char) 9999;
        }
    }
}