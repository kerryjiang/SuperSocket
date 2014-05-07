using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace SuperSocket.SocketBase.Pool
{
    struct PoolItemState
    {
        public byte Generation { get; set; }
    }

    public class IntelliPrimitiveObjectPool<T> : IntelliPoolBase<T>
    {
        private ConcurrentDictionary<T, PoolItemState> m_BufferDict = new ConcurrentDictionary<T, PoolItemState>();

        private ConcurrentDictionary<T, T> m_RemovedBufferDict;

        public IntelliPrimitiveObjectPool(int initialCount, IPoolItemCreator<T> itemCreator, Action<T> itemCleaner = null)
            : base(initialCount, itemCreator, itemCleaner)
        {

        }

        protected override void RegisterNewItem(T item)
        {
            PoolItemState state = new PoolItemState();
            state.Generation = CurrentGeneration;
            m_BufferDict.TryAdd(item, state);
        }

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

            if (m_RemovedBufferDict == null)
                m_RemovedBufferDict = new ConcurrentDictionary<T, T>();

            foreach (var item in toBeRemoved)
            {
                PoolItemState state;
                if (m_BufferDict.TryRemove(item, out state))
                    m_RemovedBufferDict.TryAdd(item, item);
            }

            return true;
        }

        protected override bool CanReturn(T item)
        {
            return m_BufferDict.ContainsKey(item);
        }

        protected override bool TryRemove(T item)
        {
            if (m_RemovedBufferDict == null || m_RemovedBufferDict.Count == 0)
                return false;

            T removedItem;
            return m_RemovedBufferDict.TryRemove(item, out removedItem);
        }
    }

    public class IntelliPool<T> : IntelliPoolBase<T>
        where T : PoolableItem<T>, IPoolableItem
    {
        public IntelliPool(int initialCount, IPoolItemCreator<T> itemCreator, Action<T> itemCleaner = null)
            : base(initialCount, itemCreator, itemCleaner)
        {

        }

        protected override void RegisterNewItem(T item)
        {
            item.Initialize(this, CurrentGeneration);
        }

        protected override bool CanReturn(T item)
        {
            return item.Generation <= CurrentGeneration;
        }

        protected override bool TryRemove(T item)
        {
            return item.Generation > CurrentGeneration;
        }
    }

    class DefaultConstructorItemCreator<TItem> : IPoolItemCreator<TItem>
            where TItem : new()
    {
        public IEnumerable<TItem> Create(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new TItem();
            }
        }
    }

    public abstract class IntelliPoolBase<T> : IPool<T>
    {
        private ConcurrentStack<T> m_Store;

        private IPoolItemCreator<T> m_ItemCreator;

        private byte m_CurrentGeneration = 0;

        protected byte CurrentGeneration
        {
            get { return m_CurrentGeneration; }
        }

        private int m_NextExpandThreshold;

        private int m_TotalCount;

        public int TotalCount
        {
            get { return m_TotalCount; }
        }

        private int m_AvailableCount;

        public int AvailableCount
        {
            get { return m_AvailableCount; }
        }

        private int m_InExpanding = 0;

        private Action<T> m_ItemCleaner;

        public IntelliPoolBase(int initialCount, IPoolItemCreator<T> itemCreator, Action<T> itemCleaner = null)
        {
            m_ItemCreator = itemCreator;
            m_ItemCleaner = itemCleaner;

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

        protected abstract void RegisterNewItem(T item);

        private void UpdateNextExpandThreshold()
        {
            m_NextExpandThreshold = m_TotalCount / 5; //if only 20% buffer left, we can expand the buffer count
        }

        public T Get()
        {
            T item;

            if (m_Store.TryPop(out item))
            {
                Interlocked.Decrement(ref m_AvailableCount);

                if (m_AvailableCount <= m_NextExpandThreshold && m_InExpanding == 0)
                    ThreadPool.QueueUserWorkItem(w => TryExpand());

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
            UpdateNextExpandThreshold();
        }

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

        protected abstract bool CanReturn(T item);

        protected abstract bool TryRemove(T item);

        public void Return(T item)
        {
            if (m_ItemCleaner != null)
                m_ItemCleaner(item);

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
