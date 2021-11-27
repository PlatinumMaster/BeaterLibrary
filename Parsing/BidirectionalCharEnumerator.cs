namespace BeaterLibrary.Parsing
{
    public class BidirectionalCharEnumerator
    {
        private readonly string Data;
        private int Index = -1;

        public BidirectionalCharEnumerator(string Data)
        {
            this.Data = Data;
        }

        public char Current => Index >= 0 && Index < Data.Length ? Data[Index] : (char) 0;

        public bool MoveNext()
        {
            return ++Index >= 0 && Index < Data.Length;
        }

        public bool MoveBack()
        {
            return --Index >= 0 && Index < Data.Length;
        }

        public bool HasNext()
        {
            return Index + 1 >= 0 && Index + 1 < Data.Length;
        }

        public bool HasPrev()
        {
            return Index - 1 >= 0 && Index - 1 < Data.Length;
        }

        public void Reset()
        {
            Index = -1;
        }

        public char PeekNext()
        {
            if (HasNext())
            {
                char Data;
                MoveNext();
                Data = Current;
                MoveBack();
                return Data;
            }

            return (char) 9999;
        }
    }
}