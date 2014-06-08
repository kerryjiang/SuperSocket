using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketBase.Pool
{
    struct PoolItemState
    {
        public byte Generation { get; set; }
    }

    /// <summary>
    /// Intelligent object pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelliPool<T> : IntelliPoolBase<T>
    {
        private ConcurrentDictionary<T, PoolItemState> m_BufferDict = new ConcurrentDictionary<T, PoolItemState>();

        private ConcurrentDictionary<T, T> m_RemovedItemDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntelliPool{T}"/> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="itemCreator">The item creator.</param>
        /// <param name="itemCleaner">The item cleaner.</param>
        /// <param name="itemPreGet">The item pre get.</param>
        public IntelliPool(int initialCount, IPoolItemCreator<T> itemCreator, Action<T> itemCleaner = null, Action<T> itemPreGet = null)
            : base(initialCount, itemCreator, itemCleaner, itemPreGet)
        {

        }

        /// <summary>
        /// Registers the new item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void RegisterNewItem(T item)
        {
            PoolItemState state = new PoolItemState();
            state.Generation = CurrentGeneration;
            m_BufferDict.TryAdd(item, state);
        }

        /// <summary>
        /// Shrinks this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Shrink()
        {
            var generation = CurrentGeneration;

            if(!base.Shrink())
                return false;

            var toBeRemoved = new List<T>(TotalCount / 2);

            foreach (var item in m_BufferDict)
            {
                if (item.Value.Generation == generation)
                {
                    toBeRemoved.Add(item.Key);
                }
            }

            if (m_RemovedItemDict == null)
                m_RemovedItemDict = new ConcurrentDictionary<T, T>();

            foreach (var item in toBeRemoved)
            {
                PoolItemState state;
                if (m_BufferDict.TryRemove(item, out state))
                    m_RemovedItemDict.TryAdd(item, item);
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified item can be returned.
        /// </summary>
        /// <param name="item">The item to be returned.</param>
        /// <returns>
        ///   <c>true</c> if the specified item can be returned; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanReturn(T item)
        {
            return m_BufferDict.ContainsKey(item);
        }

        /// <summary>
        /// Tries to remove the specific item
        /// </summary>
        /// <param name="item">The specific item to be removed.</param>
        /// <returns></returns>
        protected override bool TryRemove(T item)
        {
            if (m_RemovedItemDict == null || m_RemovedItemDict.Count == 0)
                return false;

            T removedItem;
            return m_RemovedItemDict.TryRemove(item, out removedItem);
        }
    }

    /// <summary>
    /// The item creator using type's parameter less constructor
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    class DefaultConstructorItemCreator<TItem> : IPoolItemCreator<TItem>
            where TItem : new()
    {
        /// <summary>
        /// Creates items of the specified count.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public IEnumerable<TItem> Create(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new TItem();
            }
        }
    }

    /// <summary>
    /// Intelligent pool base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IntelliPoolBase<T> : IPool<T>
    {
        private ConcurrentStack<T> m_Store;

        private IPoolItemCreator<T> m_ItemCreator;

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        protected ILog Log { get; private set; }

        private byte m_CurrentGeneration = 0;

        /// <summary>
        /// Gets the current generation.
        /// </summary>
        /// <value>
        /// The current generation.
        /// </value>
        protected byte CurrentGeneration
        {
            get { return m_CurrentGeneration; }
        }

        private int m_NextExpandThreshold;

        private int m_TotalCount;

        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount
        {
            get { return m_TotalCount; }
        }

        private int m_AvailableCount;

        /// <summary>
        /// Gets the available count, the items count which are available to be used.
        /// </summary>
        /// <value>
        /// The available count.
        /// </value>
        public int AvailableCount
        {
            get { return m_AvailableCount; }
        }

        private int m_InExpanding = 0;

        private Action<T> m_ItemCleaner;

        private Action<T> m_ItemPreGet;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntelliPoolBase{T}"/> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="itemCreator">The item creator.</param>
        /// <param name="itemCleaner">The item cleaner.</param>
        /// <param name="itemPreGet">The item pre get.</param>
        public IntelliPoolBase(int initialCount, IPoolItemCreator<T> itemCreator, Action<T> itemCleaner = null, Action<T> itemPreGet = null)
        {
            m_ItemCreator = itemCreator;
            m_ItemCleaner = itemCleaner;
            m_ItemPreGet = itemPreGet;

            Log = AppContext.CurrentServer.Logger;

            var list = new List<T>(initialCount);

            foreach(var item in itemCreator.Create(initialCount))
            {
                RegisterNewItem(item);
                list.Add(item);
            }

            m_Store = new ConcurrentStack<T>(list);

            m_TotalCount = initialCount;
            m_AvailableCount = m_TotalCount;
            UpdateNextExpandThreshold();
        }

        /// <summary>
        /// Registers the new item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected abstract void RegisterNewItem(T item);

        private void UpdateNextExpandThreshold()
        {
            m_NextExpandThreshold = m_TotalCount / 5; //if only 20% buffer left, we can expand the buffer count
        }

        /// <summary>
        /// Gets an item from the pool.
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T item;

            if (m_Store.TryPop(out item))
            {
                Interlocked.Decrement(ref m_AvailableCount);

                if (m_AvailableCount <= m_NextExpandThreshold && m_InExpanding == 0)
                    ThreadPool.QueueUserWorkItem(w => TryExpand());

                var itemPreGet = m_ItemPreGet;

                if (itemPreGet != null)
                    itemPreGet(item);

                return item;
            }

            //In expanding
            if (m_InExpanding == 1)
            {
                var spinWait = new SpinWait();

                while (true)
                {
                    spinWait.SpinOnce();

                    if (m_Store.TryPop(out item))
                    {
                        Interlocked.Decrement(ref m_AvailableCount);

                        var itemPreGet = m_ItemPreGet;

                        if (itemPreGet != null)
                            itemPreGet(item);

                        return item;
                    }

                    if (m_InExpanding != 1)
                        return Get();
                }
            }
            else
            {
                TryExpand();
                return Get();
            }
        }

        bool TryExpand()
        {
            if (Interlocked.CompareExchange(ref m_InExpanding, 1, 0) != 0)
                return false;

            Expand();
            m_InExpanding = 0;
            return true;
        }

        void Expand()
        {
            var totalCount = m_TotalCount;

            foreach (var item in m_ItemCreator.Create(totalCount))
            {
                m_Store.Push(item);
                Interlocked.Increment(ref m_AvailableCount);
                RegisterNewItem(item);
            }

            m_CurrentGeneration++;

            m_TotalCount += totalCount;
            Log.DebugFormat("The size of the pool {0}[{1}] was expanded from {2} to {3}", this.GetType().Name, this.GetHashCode(), totalCount, m_TotalCount);
            UpdateNextExpandThreshold();
        }

        /// <summary>
        /// Shrinks this pool.
        /// </summary>
        /// <returns></returns>
        public virtual bool Shrink()
        {
            var generation = m_CurrentGeneration;
            if (generation == 0)
                return false;

            var shrinThreshold = m_TotalCount * 3 / 4;

            if (m_AvailableCount <= shrinThreshold)
                return false;

            m_CurrentGeneration = (byte)(generation - 1);
            return true;
        }

        /// <summary>
        /// Determines whether the specified item can be returned.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if the specified item can be returned; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanReturn(T item);

        /// <summary>
        /// Tries to remove the specific item
        /// </summary>
        /// <param name="item">The specific item to be removed.</param>
        /// <returns></returns>
        protected abstract bool TryRemove(T item);

        /// <summary>
        /// Returns the specified item to the pool.
        /// </summary>
        /// <param name="item">The item to be returned.</param>
        public void Return(T item)
        {
            var itemCleaner = m_ItemCleaner;
            if (itemCleaner != null)
                itemCleaner(item);

            if (CanReturn(item))
            {
                m_Store.Push(item);
                Interlocked.Increment(ref m_AvailableCount);
                return;
            }

            if (TryRemove(item))
                Interlocked.Decrement(ref m_TotalCount);
        }
    }
}
