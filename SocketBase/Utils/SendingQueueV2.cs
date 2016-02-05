using System;
using System.Collections.Generic;
using System.Threading;

namespace SuperSocket.SocketBase
{
    public class SendingQueueV2 : IList<ArraySegment<byte>>
    {
        private readonly int m_Offset;

        private readonly int m_Capacity;

        private int m_InnerOffset;

        private int m_CurrentCount = 0;

        private ArraySegment<byte>[] m_GlobalQueue;

        private static ArraySegment<byte> m_Null = default(ArraySegment<byte>);

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

        ArraySegment<byte> IList<ArraySegment<byte>>.this[int index]
        {
            get
            {
                var targetIndex = m_Offset + m_InnerOffset + index;
                var value = m_GlobalQueue[targetIndex];

                if (value.Array != null)
                    return value;

                var spinWait = new SpinWait();

                while (true)
                {
                    spinWait.SpinOnce();
                    value = m_GlobalQueue[targetIndex];

                    if (value.Array != null)
                        return value;

                    if (spinWait.Count > 50)
                        return value;
                }
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection implementation

        void ICollection<ArraySegment<byte>>.Add(ArraySegment<byte> item)
        {
            lock (this)
            {
                if (m_CurrentCount == m_Capacity)
                    return;
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
            throw new NotImplementedException();
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
                throw new NotImplementedException();
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

