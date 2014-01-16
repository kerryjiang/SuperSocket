using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    /// <summary>
    /// SendingQueue
    /// </summary>
    public sealed class SendingQueue : IList<ArraySegment<byte>>
    {
        private readonly int m_Offset;

        private readonly int m_Capacity;

        private int m_CurrentCount = 0;

        private ArraySegment<byte>[] m_GlobalQueue;

        private static ArraySegment<byte> m_Null = default(ArraySegment<byte>);

        private int m_UpdatingCount;

        private bool m_ReadOnly = false;

        private ushort m_TrackID = 1;

        private int m_InnerOffset = 0;

        /// <summary>
        /// Gets the track ID.
        /// </summary>
        /// <value>
        /// The track ID.
        /// </value>
        public ushort TrackID
        {
            get { return m_TrackID; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendingQueue" /> class.
        /// </summary>
        /// <param name="globalQueue">The global queue.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="capacity">The capacity.</param>
        public SendingQueue(ArraySegment<byte>[] globalQueue, int offset, int capacity)
        {
            m_GlobalQueue = globalQueue;
            m_Offset = offset;
            m_Capacity = capacity;
        }

        private bool TryEnqueue(ArraySegment<byte> item, out bool conflict, ushort trackID)
        {
            conflict = false;

            var oldCount = m_CurrentCount;

            if (oldCount >= Capacity)
                return false;

            if (m_ReadOnly)
                return false;

            if (trackID != m_TrackID)
                return false;

            int compareCount = Interlocked.CompareExchange(ref m_CurrentCount, oldCount + 1, oldCount);

            //conflicts
            if (compareCount != oldCount)
            {
                conflict = true;
                return false;
            }

            m_GlobalQueue[m_Offset + oldCount] = item;

            return true;
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="trackID">The track ID.</param>
        /// <returns></returns>
        public bool Enqueue(ArraySegment<byte> item, ushort trackID)
        {
            if (m_ReadOnly)
                return false;

            Interlocked.Increment(ref m_UpdatingCount);

            while (true)
            {
                bool conflict = false;

                if (TryEnqueue(item, out conflict, trackID))
                {
                    Interlocked.Decrement(ref m_UpdatingCount);
                    return true;
                }

                //Needn't retry
                if (!conflict)
                    break;
            }

            Interlocked.Decrement(ref m_UpdatingCount);
            return false;
        }

        /// <summary>
        /// Enqueues the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="trackID">The track ID.</param>
        /// <returns></returns>
        public bool Enqueue(IList<ArraySegment<byte>> items, ushort trackID)
        {
            if (m_ReadOnly)
                return false;

            Interlocked.Increment(ref m_UpdatingCount);

            bool conflict;

            while (true)
            {
                if (TryEnqueue(items, out conflict, trackID))
                    break;

                if (!conflict)
                {
                    Interlocked.Decrement(ref m_UpdatingCount);
                    return false;
                }
            }

            Interlocked.Decrement(ref m_UpdatingCount);
            return true;
        }

        private bool TryEnqueue(IList<ArraySegment<byte>> items, out bool conflict, ushort trackID)
        {
            conflict = false;

            var oldCount = m_CurrentCount;

            int newItemCount = items.Count;
            int expectedCount = oldCount + newItemCount;

            if (expectedCount > Capacity)
                return false;

            if (m_ReadOnly)
                return false;

            if (m_TrackID != trackID)
                return false;

            int compareCount = Interlocked.CompareExchange(ref m_CurrentCount, expectedCount, oldCount);

            if (compareCount != oldCount)
            {
                conflict = true;
                return false;
            }

            var queue = m_GlobalQueue;

            for (var i = 0; i < items.Count; i++)
            {
                queue[m_Offset + oldCount + i] = items[i];
            }

            return true;
        }

        /// <summary>
        /// Stops the enqueue, and then wait all current excueting enqueu threads exit.
        /// </summary>
        public void StopEnqueue()
        {
            if (m_ReadOnly)
                return;

            m_ReadOnly = true;

            if (m_UpdatingCount <= 0)
                return;

            var spinWait = new SpinWait();

            spinWait.SpinOnce();

            //Wait until all insertings are finished
            while (m_UpdatingCount > 0)
            {
                spinWait.SpinOnce();
            }
        }

        /// <summary>
        /// Starts to allow enqueue.
        /// </summary>
        public void StartEnqueue()
        {
            m_ReadOnly = false;
        }

        /// <summary>
        /// Gets the global queue.
        /// </summary>
        /// <value>
        /// The global queue.
        /// </value>
        public ArraySegment<byte>[] GlobalQueue
        {
            get { return m_GlobalQueue; }
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public int Offset
        {
            get { return m_Offset; }
        }

        /// <summary>
        /// Gets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        public int Capacity
        {
            get { return m_Capacity; }
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
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public int Position { get; set; }

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
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Clear()
        {
            if (m_TrackID >= ushort.MaxValue)
                m_TrackID = 1;
            else
                m_TrackID++;
            
            for (var i = 0; i < m_CurrentCount; i++)
            {
                m_GlobalQueue[m_Offset + i] = m_Null;
            }

            m_CurrentCount = 0;
            m_InnerOffset = 0;
            Position = 0;
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
        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            for (var i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return m_ReadOnly; }
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
        /// <exception cref="System.NotSupportedException"></exception>
        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            for (var i = 0; i < (m_CurrentCount - m_InnerOffset); i++)
            {
                yield return m_GlobalQueue[m_Offset + m_InnerOffset + i];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
    }


    /// <summary>
    /// SendingQueueSourceCreator
    /// </summary>
    public class SendingQueueSourceCreator : ISmartPoolSourceCreator<SendingQueue>
    {
        private int m_SendingQueueSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendingQueueSourceCreator" /> class.
        /// </summary>
        /// <param name="sendingQueueSize">Size of the sending queue.</param>
        public SendingQueueSourceCreator(int sendingQueueSize)
        {
            m_SendingQueueSize = sendingQueueSize;
        }

        /// <summary>
        /// Creates the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="poolItems">The pool items.</param>
        /// <returns></returns>
        public ISmartPoolSource Create(int size, out SendingQueue[] poolItems)
        {
            var source = new ArraySegment<byte>[size * m_SendingQueueSize];

            poolItems = new SendingQueue[size];

            for (var i = 0; i < size; i++)
            {
                poolItems[i] = new SendingQueue(source, i * m_SendingQueueSize, m_SendingQueueSize);
            }

            return new SmartPoolSource(source, size);
        }
    }
}
