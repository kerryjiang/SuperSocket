using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Concurrent
{
    public class ConcurrentDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> m_InnerDict;

        private object m_SyncRoot = new object();

        public ConcurrentDictionary()
        {
            m_InnerDict = new Dictionary<TKey, TValue>();
        }

        public ConcurrentDictionary(int capacity)
        {
            m_InnerDict = new Dictionary<TKey, TValue>(capacity);
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            m_InnerDict = new Dictionary<TKey, TValue>(comparer);
        }

        public ConcurrentDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            m_InnerDict = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (m_SyncRoot)
            {
                if (m_InnerDict.ContainsKey(key))
                    return false;

                m_InnerDict.Add(key, value);
                return true;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (m_SyncRoot)
            {
                return m_InnerDict.TryGetValue(key, out value);
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (m_SyncRoot)
            {
                if (!m_InnerDict.TryGetValue(key, out value))
                    return false;

                return m_InnerDict.Remove(key);
            }
        }

        public int Count
        {
            get { return m_InnerDict.Count; }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (m_SyncRoot)
                {
                    return m_InnerDict.Values.ToArray();
                }
            }
        }
    }
}
