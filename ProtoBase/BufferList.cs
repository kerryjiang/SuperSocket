using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The receive cache
    /// </summary>
    public sealed class BufferList : IList<ArraySegment<byte>>
    {
        private List<KeyValuePair<ArraySegment<byte>, IBufferState>> m_List = new List<KeyValuePair<ArraySegment<byte>, IBufferState>>();

        /// <summary>
        /// Gets the last buffer segment.
        /// </summary>
        /// <value>
        /// The last.
        /// </value>
        public ArraySegment<byte> Last
        {
            get
            {
                var count = Count;

                if (count == 0)
                    return new ArraySegment<byte>();

                return m_List[count - 1].Key;
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public int IndexOf(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public ArraySegment<byte> this[int index]
        {
            get
            {
                return m_List[index].Key;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Add(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="state">The state.</param>
        public void Add(ArraySegment<byte> item, IBufferState state)
        {
            state.IncreaseReference();
            m_List.Add(new KeyValuePair<ArraySegment<byte>, IBufferState>(item, state));
            m_Total += item.Count;
        }

        /// <summary>
        /// Sets the last length of the item.
        /// </summary>
        /// <param name="length">The length.</param>
        public void SetLastItemLength(int length)
        {
            var lastPos = m_List.Count - 1;
            var lastItem = m_List[lastPos];
            var lastSegment = lastItem.Key;
            m_List[lastPos] = new KeyValuePair<ArraySegment<byte>, IBufferState>(new ArraySegment<byte>(lastSegment.Array, lastSegment.Offset, length), lastItem.Value);
            m_Total += length - lastSegment.Count;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            m_List.Clear();
            m_Total = 0;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public bool Contains(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        private int m_Total;

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public int Total
        {
            get { return m_Total; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///   </returns>
        public int Count
        {
            get { return m_List.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        ///   </returns>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public bool Remove(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            var length = m_List.Count;

            for (int i = 0; i < length; i++)
            {
                yield return m_List[i].Key;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets all cached items.
        /// </summary>
        /// <returns></returns>
        public IList<KeyValuePair<ArraySegment<byte>, IBufferState>> GetAllCachedItems()
        {
            return m_List;
        }

        internal BufferList Clone(int index, int segmentOffset, int length)
        {
            var target = new BufferList();

            var rest = length;

            var segments = m_List;

            for (var i = index; i < segments.Count; i++)
            {
                var pair = segments[i];
                var segment = pair.Key;
                var offset = segment.Offset;
                var thisLen = segment.Count;

                if (i == index)
                {
                    offset = segmentOffset;
                    thisLen = segment.Count - (segmentOffset - segment.Offset);
                }

                thisLen = Math.Min(thisLen, rest);

                target.Add(new ArraySegment<byte>(segment.Array, offset, thisLen), pair.Value);

                rest -= thisLen;

                if (rest <= 0)
                    break;
            }

            return target;
        }
    }
}
