using System;
using System.Collections.Generic;
using System.Text;

namespace BeaterLibrary.Parsing
{
    public class BidirectionalCharEnumerator
    {
        private int Index = -1;
        private string Data;

        public char Current
        {
            get => Index >= 0 && Index < Data.Length ? Data[Index] : (char) 0;
        }

        public BidirectionalCharEnumerator(string Data) => this.Data = Data;
        public bool MoveNext() => ++Index >= 0 && Index < Data.Length;
        public bool MoveBack() => --Index >= 0 && Index < Data.Length;
        public bool HasNext() => Index + 1 >= 0 && Index + 1 < Data.Length;
        public bool HasPrev() => Index - 1 >= 0 && Index - 1 < Data.Length;
        public void Reset() => Index = -1;
    }
}