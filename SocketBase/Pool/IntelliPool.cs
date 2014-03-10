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
        public GCHandle GCHandle { get; set; }

        public byte Generation { get; set; }
    }

    public class IntelliPool<T> : IPool<T>
    {
        private ConcurrentStack<T> m_Store;

        private IPoolItemCreator<T> m_ItemCreator;

        private byte m_CurrentGeneration = 0;

        private ConcurrentDictionary<T, PoolItemState> m_BufferDict;

        private ConcurrentDictionary<T, GCHandle> m_RemovedBufferDict;

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

        public IntelliPool(int initialCount, IPoolItemCreator<T> itemCreator)
        {
            m_ItemCreator = itemCreator;
            m_BufferDict = new ConcurrentDictionary<T, PoolItemState>();

            var list = new List<T>(initialCount);

            foreach(var item in itemCreator.Create(initialCount))
            {
                var handle = GCHandle.Alloc(item, GCHandleType.Pinned); //Pinned the buffer in the memory
                PoolItemState state = new PoolItemState();
                state.GCHandle = handle;
                state.Generation = m_CurrentGeneration;
                m_BufferDict.TryAdd(item, state);
                list.Add(item);
            }

            m_Store = new ConcurrentStack<T>(list);

            m_TotalCount = initialCount;
            m_AvailableCount = m_TotalCount;
            UpdateNextExpandThreshold();
        }

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
            return true;
        }

        void Expand()
        {
            var totalCount = m_TotalCount;

            foreach (var item in m_ItemCreator.Create(totalCount))
            {
                var handle = GCHandle.Alloc(item, GCHandleType.Pinned); //Pinned the buffer in the memory

                m_Store.Push(item);
                Interlocked.Increment(ref m_AvailableCount);

                PoolItemState state = new PoolItemState();
                state.GCHandle = handle;
                state.Generation = m_CurrentGeneration;
                m_BufferDict.TryAdd(item, state);
            }

            m_CurrentGeneration++;

            m_TotalCount += totalCount;
            UpdateNextExpandThreshold();
            m_InExpanding = 0;
        }

        public void Shrink()
        {
            var generation = m_CurrentGeneration;
            if (generation == 0)
                return;

            var shrinThreshold = m_TotalCount * 3 / 4;

            if (m_AvailableCount <= shrinThreshold)
                return;

            m_CurrentGeneration = (byte)(generation - 1);

            var toBeRemoved = new List<T>(m_TotalCount / 2);

            foreach (var item in m_BufferDict)
            {
                if (item.Value.Generation == generation)
                {
                    toBeRemoved.Add(item.Key);
                }
            }

            if (m_RemovedBufferDict == null)
                m_RemovedBufferDict = new ConcurrentDictionary<T, GCHandle>();

            foreach (var item in toBeRemoved)
            {
                PoolItemState state;
                if (m_BufferDict.TryRemove(item, out state))
                    m_RemovedBufferDict.TryAdd(item, state.GCHandle);
            }
        }

        public void Return(T item)
        {
            if (m_BufferDict.ContainsKey(item))
            {
                m_Store.Push(item);
                Interlocked.Increment(ref m_AvailableCount);
                return;
            }

            if (m_RemovedBufferDict == null)
                return;

            GCHandle handle;

            if (m_RemovedBufferDict.TryRemove(item, out handle))
            {
                Interlocked.Decrement(ref m_TotalCount);
                handle.Free(); //Change the buffer to Normal from Pinned in the memory
            }
        }
    }
}
