using System;
using System.Collections.Generic;
using System.Threading;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// SendingQueueV2
    /// </summary>
    public class SendingQueueV2 : ISendingQueue
    {
        private readonly int m_Offset;

        private readonly int m_Capacity;

        private int m_InnerOffset;

        private int m_CurrentCount = 0;

        private ArraySegment<byte>[] m_GlobalQueue;

        private static ArraySegment<byte> m_Null = default(ArraySegment<byte>);

        /// <summary>
        /// SendingQueueV2
        /// </summary>
        /// <param name="globalQueue"></param>
        /// <param name="offset"></param>
        /// <param name="capacity"></param>
        public SendingQueueV2(ArraySegment<byte>[] globalQueue, int offset, int capacity)
        {
            m_GlobalQueue = globalQueue;
            m_Offset = offset;
            m_Capacity = capacity;
        }

        #region IList implementation

        int IList<ArraySegment<byte>>.IndexOf(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        void IList<ArraySegment<byte>>.Insert(int index, ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        void IList<ArraySegment<byte>>.RemoveAt(int index)
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
                var targetIndex = m_Offset + m_InnerOffset + index;
                return m_GlobalQueue[targetIndex];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection implementation

        /// <summary>
        /// add single one item into the queue
        /// </summary>
        /// <param name="item">the item to be inserted</param>
        public void Add(ArraySegment<byte> item)
        {
            lock (this)
            {
                if (m_CurrentCount == m_Capacity)
                {
                    throw new Exception("This sending queue is full, you cannot insert items into it any more.");
                }

                var oldCount = m_CurrentCount;
                m_GlobalQueue[m_Offset + oldCount] = item;
                m_CurrentCount = oldCount + 1;
            }
        }

        /// <summary>
        /// add multiple items into the queue
        /// </summary>
        /// <param name="itemsSource">the source cof the multiple items to be inserted</param>
        public void AddRange(Func<IList<ArraySegment<byte>>> itemsSource)
        {
            lock (this)
            {
                var items = itemsSource();

                if ((m_CurrentCount + items.Count) > m_Capacity)
                {
                    throw new Exception("This sending queue is going to be full, you cannot insert items into it.");
                }

                var oldCount = m_CurrentCount;

                for (var i = 0; i < items.Count; i++)
                {
                    m_GlobalQueue[m_Offset + oldCount + i] = items[i];
                }
                
                m_CurrentCount = oldCount + items.Count;
            }
        }

        void ICollection<ArraySegment<byte>>.Clear()
        {
            for (var i = 0; i < m_CurrentCount; i++)
            {
                m_GlobalQueue[m_Offset + i] = m_Null;
            }

            m_CurrentCount = 0;
            m_InnerOffset = 0;
        }

        bool ICollection<ArraySegment<byte>>.Contains(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<ArraySegment<byte>>.CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            lock(this)
            {
                for (var i = 0; i < Count; i++)
                {
                    array[arrayIndex + i] = this[i];
                }
            }
        }

        bool ICollection<ArraySegment<byte>>.Remove(ArraySegment<byte> item)
        {
            throw new NotSupportedException();
        }

        int ICollection<ArraySegment<byte>>.Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection<ArraySegment<byte>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count
        {
            get { return m_CurrentCount - m_InnerOffset; }
        }

        /// <summary>
        /// Trim the internal segments at the begining by the binary data size.
        /// </summary>
        /// <param name="offset">The binary data size should be trimed at the begining.</param>
        public void InternalTrim(int offset)
        {
            var innerCount = m_CurrentCount - m_InnerOffset;
            var subTotal = 0;

            for (var i = m_InnerOffset; i < innerCount; i++)
            {
                var segment = m_GlobalQueue[m_Offset + i];
                subTotal += segment.Count;

                if (subTotal <= offset)
                    continue;

                m_InnerOffset = i;

                var rest = subTotal - offset;
                m_GlobalQueue[m_Offset + i] = new ArraySegment<byte>(segment.Array, segment.Offset + segment.Count - rest, rest);

                break;
            }
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator<ArraySegment<byte>> IEnumerable<ArraySegment<byte>>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion
        
    }
}

