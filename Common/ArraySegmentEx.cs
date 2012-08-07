using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    class ArraySegmentEx<T>
    {
        public ArraySegmentEx(T[] array, int offset, int count)
        {
            Array = array;
            Offset = offset;
            Count = count;
        }
        /// <summary>
        /// Gets the array.
        /// </summary>
        public T[] Array { get; private set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset { get; private set; }

        public int From { get; set; }

        public int To { get; set; }
    }
}
