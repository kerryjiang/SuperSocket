using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    /// <summary>
    /// ArraySegmentList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ArraySegmentList<T> : IList<T>
        where T : IEquatable<T>
    {
        private IList<ArraySegmentEx<T>> m_Segments;

        internal IList<ArraySegmentEx<T>> Segments
        {
            get { return m_Segments; }
        }

        private ArraySegmentEx<T> m_PrevSegment;
        private int m_PrevSegmentIndex;

        private int m_Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArraySegmentList&lt;T&gt;"/> class.
        /// </summary>
        public ArraySegmentList()
        {
            m_Segments = new List<ArraySegmentEx<T>>();
        }

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            int index = 0;

            for (int i = 0; i < m_Segments.Count; i++)
            {
                var currentSegment = m_Segments[i];
                int offset = currentSegment.Offset;

                for (int j = 0; j < currentSegment.Count; j++)
                {
                    if (currentSegment.Array[j + offset].Equals(item))
                        return index;

                    index++;
                }
            }

            return -1;
        }

        /// <summary>
        /// NotSupported
        /// </summary>
        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NotSupported
        /// </summary>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>
        /// The element at the specified index.
        ///   </returns>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
        ///   </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">
        /// The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
        ///   </exception>
        public T this[int index]
        {
            get
            {
                ArraySegmentEx<T> segment;

                var internalIndex = GetElementInternalIndex(index, out segment);

                if (internalIndex < 0)
                    throw new IndexOutOfRangeException();

                return segment.Array[internalIndex];
            }
            set
            {
                ArraySegmentEx<T> segment;

                var internalIndex = GetElementInternalIndex(index, out segment);

                if (internalIndex < 0)
                    throw new IndexOutOfRangeException();

                segment.Array[internalIndex] = value;
            }
        }

        private int GetElementInternalIndex(int index, out ArraySegmentEx<T> segment)
        {
            segment = null;

            if (index < 0 || index > Count - 1)
                return -1;

            if (index == 0)
            {
                m_PrevSegment = m_Segments[0];
                m_PrevSegmentIndex = 0;
                segment = m_PrevSegment;
                return m_PrevSegment.Offset;
            }

            int compareValue = 0;

            if (m_PrevSegment != null)
            {
                if (index >= m_PrevSegment.From)
                {
                    if (index <= m_PrevSegment.To)
                    {
                        segment = m_PrevSegment;
                        return m_PrevSegment.Offset + index - m_PrevSegment.From;
                    }
                    else
                    {
                        compareValue = 1;
                    }
                }
                else
                {
                    compareValue = -1;
                }
            }

            int from, to;

            if (compareValue != 0)
            {
                from = m_PrevSegmentIndex + compareValue;

                var trySegment = m_Segments[from];

                if (index >= trySegment.From && index <= trySegment.To)
                {
                    segment = trySegment;
                    return trySegment.Offset + index - trySegment.From;
                }

                from += compareValue;

                var currentSegment = m_Segments[from];
                if (index >= currentSegment.From && index <= currentSegment.To)
                {
                    m_PrevSegment = currentSegment;
                    m_PrevSegmentIndex = from;
                    segment = currentSegment;
                    return currentSegment.Offset + index - currentSegment.From;
                }

                if (compareValue > 0)
                {
                    from++;
                    to = m_Segments.Count - 1;
                }
                else
                {
                    var tmp = from - 1;
                    from = 0;
                    to = tmp;
                }
            }
            else
            {
                from = 0;
                to = m_Segments.Count - 1;
            }

            int segmentIndex = -1;

            var result = QuickSearchSegment(from, to, index, out segmentIndex);

            if (result != null)
            {
                m_PrevSegment = result;
                m_PrevSegmentIndex = segmentIndex;
                segment = m_PrevSegment;
                return result.Offset + index - result.From;
            }

            m_PrevSegment = null;

            return -1;
        }

        internal ArraySegmentEx<T> QuickSearchSegment(int from, int to, int index, out int segmentIndex)
        {
            ArraySegmentEx<T> segment;
            segmentIndex = -1;

            int diff = to - from;

            if (diff == 0)
            {
                segment = m_Segments[from];

                if (index >= segment.From && index <= segment.To)
                {
                    segmentIndex = from;
                    return segment;
                }

                return null;
            }
            else if (diff == 1)
            {
                segment = m_Segments[from];

                if (index >= segment.From && index <= segment.To)
                {
                    segmentIndex = from;
                    return segment;
                }

                segment = m_Segments[to];

                if (index >= segment.From && index <= segment.To)
                {
                    segmentIndex = to;
                    return segment;
                }

                return null;
            }

            int middle = from + diff / 2;

            segment = m_Segments[middle];

            if (index >= segment.From)
            {
                if (index <= segment.To)
                {
                    segmentIndex = middle;
                    return segment;
                }
                else
                {
                    return QuickSearchSegment(middle + 1, to, index, out segmentIndex);
                }
            }
            else
            {
                return QuickSearchSegment(from, middle - 1, index, out segmentIndex);
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// NotSupported
        /// </summary>
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NotSupported
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// NotSupported
        /// </summary>
        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, 0, arrayIndex, Math.Min(array.Length, this.Count - arrayIndex));
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        ///   </returns>
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        ///   </returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// NotSupported
        /// </summary>
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// NotSupported
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// NotSupported
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// Removes the segment at.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveSegmentAt(int index)
        {
            var removedSegment = m_Segments[index];
            int removedLen = removedSegment.To - removedSegment.From + 1;

            m_Segments.RemoveAt(index);

            m_PrevSegment = null;

            //the removed item is not the the last item 
            if (index != m_Segments.Count)
            {
                for (int i = index; i < m_Segments.Count; i++)
                {
                    m_Segments[i].From -= removedLen;
                    m_Segments[i].To -= removedLen;
                }
            }

            m_Count -= removedLen;
        }

        /// <summary>
        /// Adds the segment to the list.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void AddSegment(T[] array, int offset, int length)
        {
            AddSegment(array, offset, length, false);
        }

        /// <summary>
        /// Adds the segment to the list.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        public void AddSegment(T[] array, int offset, int length, bool toBeCopied)
        {
            if (length <= 0)
                return;

            var currentTotal = m_Count;

            ArraySegmentEx<T> segment = null;

            if (!toBeCopied)
                segment = new ArraySegmentEx<T>(array, offset, length);
            else
                segment = new ArraySegmentEx<T>(array.CloneRange(offset, length), 0, length);

            segment.From = currentTotal;
            m_Count = currentTotal + segment.Count;
            segment.To = m_Count - 1;

            m_Segments.Add(segment);
        }

        /// <summary>
        /// Gets the segment count.
        /// </summary>
        public int SegmentCount
        {
            get { return m_Segments.Count; }
        }

        /// <summary>
        /// Clears all the segements.
        /// </summary>
        public void ClearSegements()
        {
            m_Segments.Clear();
            m_PrevSegment = null;
            m_Count = 0;
        }

        /// <summary>
        /// Read all data in this list to the array data.
        /// </summary>
        /// <returns></returns>
        public T[] ToArrayData()
        {
            return ToArrayData(0, m_Count);
        }

        /// <summary>
        /// Read the data in specific range to the array data.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public T[] ToArrayData(int startIndex, int length)
        {
            var result = new T[length];
            int from = 0, len = 0, total = 0;

            var startSegmentIndex = 0;

            if (startIndex != 0)
            {
                var startSegment = QuickSearchSegment(0, m_Segments.Count - 1, startIndex, out startSegmentIndex);

                if (startSegment == null)
                    throw new IndexOutOfRangeException();

                from = startIndex - startSegment.From;
            }

            for (var i = startSegmentIndex; i < m_Segments.Count; i++)
            {
                var currentSegment = m_Segments[i];
                len = Math.Min(currentSegment.Count - from, length - total);
                Array.Copy(currentSegment.Array, currentSegment.Offset + from, result, total, len);
                total += len;

                if (total >= length)
                    break;

                from = 0;
            }

            return result;
        }

        /// <summary>
        /// Trims the end.
        /// </summary>
        /// <param name="trimSize">Size of the trim.</param>
        public void TrimEnd(int trimSize)
        {
            if (trimSize <= 0)
                return;

            int expectedTo = this.Count - trimSize - 1;

            for (int i = m_Segments.Count - 1; i >= 0; i--)
            {
                var s = m_Segments[i];

                if (s.From <= expectedTo && expectedTo < s.To)
                {
                    s.To = expectedTo;
                    m_Count -= trimSize;
                    return;
                }

                RemoveSegmentAt(i);
            }
        }

        /// <summary>
        /// Searches the last segment.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public int SearchLastSegment(SearchMarkState<T> state)
        {
            if (m_Segments.Count <= 0)
                return -1;

            var lastSegment = m_Segments[m_Segments.Count - 1];

            if (lastSegment == null)
                return -1;

            var result = lastSegment.Array.SearchMark(lastSegment.Offset, lastSegment.Count, state.Mark);

            if (!result.HasValue)
                return -1;

            if (result.Value > 0)
            {
                state.Matched = 0;
                return result.Value - lastSegment.Offset + lastSegment.From;
            }

            state.Matched = 0 - result.Value;
            return -1;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public int CopyTo(T[] to)
        {
            return CopyTo(to, 0, 0, Math.Min(m_Count, to.Length));
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="srcIndex">Index of the SRC.</param>
        /// <param name="toIndex">To index.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public int CopyTo(T[] to, int srcIndex, int toIndex, int length)
        {
            int copied = 0;
            int thisCopied = 0;

            int offsetSegmentIndex;
            ArraySegmentEx<T> offsetSegment;

            if (srcIndex > 0)
                offsetSegment = QuickSearchSegment(0, m_Segments.Count - 1, srcIndex, out offsetSegmentIndex);
            else
            {
                offsetSegment = m_Segments[0];
                offsetSegmentIndex = 0;
            }

            int thisOffset = srcIndex - offsetSegment.From + offsetSegment.Offset;
            thisCopied = Math.Min(offsetSegment.Count - thisOffset + offsetSegment.Offset, length - copied);

            Array.Copy(offsetSegment.Array, thisOffset, to, copied + toIndex, thisCopied);

            copied += thisCopied;

            if (copied >= length)
                return copied;

            for (var i = offsetSegmentIndex + 1; i < this.m_Segments.Count; i++)
            {
                var segment = m_Segments[i];
                thisCopied = Math.Min(segment.Count, length - copied);
                Array.Copy(segment.Array, segment.Offset, to, copied + toIndex, thisCopied);
                copied += thisCopied;

                if (copied >= length)
                    break;
            }

            return copied;
        }
    }

    /// <summary>
    /// ArraySegmentList
    /// </summary>
    public class ArraySegmentList : ArraySegmentList<byte>
    {
        /// <summary>
        /// Decodes bytes to string by the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public string Decode(Encoding encoding)
        {
            return Decode(encoding, 0, Count);
        }

        /// <summary>
        /// Decodes bytes to string by the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public string Decode(Encoding encoding, int offset, int length)
        {
            var arraySegments = Segments;

            if (arraySegments == null || arraySegments.Count <= 0)
                return string.Empty;

            var charsBuffer = new char[encoding.GetMaxCharCount(this.Count)];

            int bytesUsed, charsUsed;
            bool completed;
            int totalBytes = 0;
            int totalChars = 0;

            int lastSegIndex = arraySegments.Count - 1;
            var flush = false;

            var decoder = encoding.GetDecoder();

            int startSegmentIndex = 0;

            if (offset > 0)
            {
                QuickSearchSegment(0, arraySegments.Count - 1, offset, out startSegmentIndex);
            }

            for (var i = startSegmentIndex; i < arraySegments.Count; i++)
            {
                var segment = arraySegments[i];

                if (i == lastSegIndex)
                    flush = true;

                int decodeOffset = segment.Offset;
                int toBeDecoded = Math.Min(length - totalBytes, segment.Count);

                if (i == startSegmentIndex && offset > 0)
                {
                    decodeOffset = offset - segment.From + segment.Offset;
                    toBeDecoded = Math.Min(segment.Count - offset + segment.From, toBeDecoded);
                }

                decoder.Convert(segment.Array, decodeOffset, toBeDecoded, charsBuffer, totalChars, charsBuffer.Length - totalChars, flush, out bytesUsed, out charsUsed, out completed);
                totalChars += charsUsed;
                totalBytes += bytesUsed;

                if (totalBytes >= length)
                    break;
            }

            return new string(charsBuffer, 0, totalChars);
        }

        /// <summary>
        /// Decodes data by the mask.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void DecodeMask(byte[] mask, int offset, int length)
        {
            int maskLen = mask.Length;
            var startSegmentIndex = 0;
            var startSegment = QuickSearchSegment(0, Segments.Count - 1, offset, out startSegmentIndex);

            var shouldDecode = Math.Min(length, startSegment.Count - offset + startSegment.From);
            var from = offset - startSegment.From + startSegment.Offset;

            var index = 0;

            for (var i = from; i < from + shouldDecode; i++)
            {
                startSegment.Array[i] = (byte)(startSegment.Array[i] ^ mask[index++ % maskLen]);
            }

            if (index >= length)
                return;

            for (var i = startSegmentIndex + 1; i < SegmentCount; i++)
            {
                var segment = Segments[i];

                shouldDecode = Math.Min(length - index, segment.Count);

                for (var j = segment.Offset; j < segment.Offset + shouldDecode; j++)
                {
                    segment.Array[j] = (byte)(segment.Array[j] ^ mask[index++ % maskLen]);
                }

                if (index >= length)
                    return;
            }
        }
    }
}
