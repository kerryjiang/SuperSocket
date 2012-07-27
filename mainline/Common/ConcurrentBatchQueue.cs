using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    /// <summary>
    /// Concurrent BatchQueue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentBatchQueue<T> : IBatchQueue<T>
    {
        private Entity m_Entity;
        private Entity m_BackEntity;

        private static readonly T m_Null = default(T);

        class Entity
        {
            public T[] Array { get; set; }

            public int Count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentBatchQueue&lt;T&gt;"/> class.
        /// </summary>
        public ConcurrentBatchQueue()
            : this(16)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentBatchQueue&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the queue.</param>
        public ConcurrentBatchQueue(int capacity)
        {
            m_Entity = new Entity();
            m_Entity.Array = new T[capacity];

            m_BackEntity = new Entity();
            m_BackEntity.Array = new T[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentBatchQueue&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="array">The array.</param>
        public ConcurrentBatchQueue(T[] array)
        {
            m_Entity = new Entity();
            m_Entity.Array = array;

            m_BackEntity = new Entity();
            m_BackEntity.Array = new T[array.Length];
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public bool Enqueue(T item)
        {
            bool full;

            while (true)
            {
                if (TryEnqueue(item, out full) || full)
                    break;
            }

            return !full;
        }

        private bool TryEnqueue(T item, out bool full)
        {
            full = false;

            EnsureNotRebuild();

            var entity = m_Entity;
            var array = entity.Array;
            var count = entity.Count;

            if (count >= array.Length)
            {
                full = true;
                return false;
            }

            if(entity != m_Entity)
                return false;

            int oldCount = Interlocked.CompareExchange(ref entity.Count, count + 1, count);

            if (oldCount != count)
                return false;

            array[count] = item;

            return true;
        }

        /// <summary>
        /// Enqueues the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        public bool Enqueue(IList<T> items)
        {
            bool full;

            while (true)
            {
                if (TryEnqueue(items, out full) || full)
                    break;
            }

            return !full;
        }

        private bool TryEnqueue(IList<T> items, out bool full)
        {
            full = false;

            var entity = m_Entity;
            var array = entity.Array;
            var count = entity.Count;

            int newItemCount = items.Count;
            int expectedCount = count + newItemCount;

            if (expectedCount > array.Length)
            {
                full = true;
                return false;
            }

            if (entity != m_Entity)
                return false;

            int oldCount = Interlocked.CompareExchange(ref entity.Count, expectedCount, count);

            if (oldCount != count)
                return false;

            foreach (var item in items)
            {
                array[count++] = item;
            }

            return true;
        }

        private void EnsureNotRebuild()
        {
            if (!m_Rebuilding)
                return;

            var spinWait = new SpinWait();

            while (true)
            {
                spinWait.SpinOnce();

                if (!m_Rebuilding)
                    break;
            }
        }

        private bool m_Rebuilding = false;

        /// <summary>
        /// Tries the dequeue.
        /// </summary>
        /// <param name="outputItems">The output items.</param>
        /// <returns></returns>
        public bool TryDequeue(IList<T> outputItems)
        {
            var entity = m_Entity;
            int count = entity.Count;

            if (count <= 0)
                return false;

            var spinWait = new SpinWait();

            Interlocked.Exchange(ref m_Entity, m_BackEntity);

            spinWait.SpinOnce();

            count = entity.Count;

            var array = entity.Array;

            var i = 0;

            while (true)
            {
                var item = array[i];

                if (item == null)
                {
                    while (item == null)
                    {
                        spinWait.SpinOnce();
                        item = array[i];
                    }
                }

                outputItems.Add(array[i]);
                array[i] = m_Null;


                if (entity.Count <= (i + 1))
                    break;

                i++;
            }

            m_BackEntity = entity;
            m_BackEntity.Count = 0;

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return m_Entity.Count <= 0; }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get { return m_Entity.Count; }
        }
    }
}
