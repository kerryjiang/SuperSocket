using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    public abstract class PoolableItem<TPoolItem> : IPoolableItem<TPoolItem>, IPoolableItem
        where TPoolItem : class, IPoolableItem
    {
        internal void Initialize(IPool<TPoolItem> pool, int generation)
        {
            Pool = pool;
            Generation = generation;
        }

        public IPool<TPoolItem> Pool { get; private set; }

        public int Generation { get; private set; }

        ~PoolableItem()
        {
            Pool.Return(this as TPoolItem);
        }
    }
}
