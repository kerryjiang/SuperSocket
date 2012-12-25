using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.Common
{
    /// <summary>
    /// The basic interface of smart pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISmartPool<T>
    {
        /// <summary>
        /// Initializes the specified min pool size.
        /// </summary>
        /// <param name="minPoolSize">The min size of the pool.</param>
        /// <param name="maxPoolSize">The max size of the pool.</param>
        /// <param name="sourceCreator">The source creator.</param>
        /// <returns></returns>
        void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator);

        /// <summary>
        /// Gets the min size of the pool.
        /// </summary>
        /// <value>
        /// The min size of the pool.
        /// </value>
        int MinPoolSize { get; }

        /// <summary>
        /// Gets the max size of the pool.
        /// </summary>
        /// <value>
        /// The max size of the pool.
        /// </value>
        int MaxPoolSize { get; }


        /// <summary>
        /// Gets the avialable items count.
        /// </summary>
        /// <value>
        /// The avialable items count.
        /// </value>
        int AvialableItemsCount { get; }

        /// <summary>
        /// Pushes the specified item into the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        void Push(T item);

        /// <summary>
        /// Tries to get one item from the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        bool TryGet(out T item);
    }

    /// <summary>
    /// ISmartPoolSource
    /// </summary>
    public interface ISmartPoolSource
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }
    }

    /// <summary>
    /// SmartPoolSource
    /// </summary>
    public class SmartPoolSource : ISmartPoolSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmartPoolSource" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="itemsCount">The items count.</param>
        public SmartPoolSource(object source, int itemsCount)
        {
            Source = source;
            Count = itemsCount;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public object Source { get; private set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }
    }



    /// <summary>
    /// ISmartPoolSourceCreator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISmartPoolSourceCreator<T>
    {
        /// <summary>
        /// Creates the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="poolItems">The pool items.</param>
        /// <returns></returns>
        ISmartPoolSource Create(int size, out T[] poolItems);
    }

    /// <summary>
    /// The smart pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartPool<T> : ISmartPool<T>
    {
        private ConcurrentStack<T> m_GlobalStack;

        private ISmartPoolSource[] m_ItemsSource;

        private int m_CurrentSourceCount;

        private ISmartPoolSourceCreator<T> m_SourceCreator;

        private int m_MinPoolSize;

        /// <summary>
        /// Gets the size of the min pool.
        /// </summary>
        /// <value>
        /// The size of the min pool.
        /// </value>
        public int MinPoolSize
        {
            get
            {
                return m_MinPoolSize;
            }
        }

        private int m_MaxPoolSize;

        /// <summary>
        /// Gets the size of the max pool.
        /// </summary>
        /// <value>
        /// The size of the max pool.
        /// </value>
        public int MaxPoolSize
        {
            get
            {
                return m_MaxPoolSize;
            }
        }

        /// <summary>
        /// Gets the avialable items count.
        /// </summary>
        /// <value>
        /// The avialable items count.
        /// </value>
        public int AvialableItemsCount
        {
            get
            {
                return m_GlobalStack.Count;
            }
        }

        /// <summary>
        /// Initializes the specified min and max pool size.
        /// </summary>
        /// <param name="minPoolSize">The min size of the pool.</param>
        /// <param name="maxPoolSize">The max size of the pool.</param>
        /// <param name="sourceCreator">The source creator.</param>
        public void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator)
        {
            m_MinPoolSize = minPoolSize;
            m_MaxPoolSize = maxPoolSize;
            m_SourceCreator = sourceCreator;
            m_GlobalStack = new ConcurrentStack<T>();

            var n = 0;

            if (minPoolSize != maxPoolSize)
            {
                var currentValue = minPoolSize;

                while (true)
                {
                    n++;

                    var thisValue = currentValue * 2;

                    if (thisValue >= maxPoolSize)
                        break;

                    currentValue = thisValue;
                }
            }

            m_ItemsSource = new ISmartPoolSource[n + 1];

            T[] items;
            m_ItemsSource[0] = sourceCreator.Create(minPoolSize, out items);
            m_CurrentSourceCount = 1;

            for (var i = 0; i < items.Length; i++)
            {
                m_GlobalStack.Push(items[i]);
            }
        }

        /// <summary>
        /// Pushes the specified item into the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Push(T item)
        {
            m_GlobalStack.Push(item);
        }

        /// <summary>
        /// Tries to get one item from the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool TryGet(out T item)
        {
            if (m_GlobalStack.TryPop(out item))
                return true;

            var currentSourceCount = m_CurrentSourceCount;

            if (currentSourceCount >= m_ItemsSource.Length)
                return false;

            if (Interlocked.CompareExchange(ref m_CurrentSourceCount, currentSourceCount + 1, currentSourceCount) != currentSourceCount)
            {
                var spinWait = new SpinWait();

                while (true)
                {
                    spinWait.SpinOnce();

                    if (m_GlobalStack.TryPop(out item))
                        return true;

                    if (spinWait.Count >= 100)
                        return false;
                }
            }

            int totalItemsCount = 0;

            for (var i = 0; i < currentSourceCount; i++)
            {
                totalItemsCount += m_ItemsSource[i].Count;
            }

            totalItemsCount = Math.Min(totalItemsCount, m_MaxPoolSize - totalItemsCount);

            T[] items;
            m_ItemsSource[currentSourceCount] = m_SourceCreator.Create(totalItemsCount, out items);

            for (var i = 0; i < items.Length; i++)
            {
                m_GlobalStack.Push(items[i]);
            }

            return m_GlobalStack.TryPop(out item);
        }
    }
}
