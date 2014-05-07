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
            if (pool == null)
                throw new ArgumentNullException("pool");

            Pool = pool;
            Generation = generation;
        }

        public IPool<TPoolItem> Pool { get; private set; }

        public int Generation { get; private set; }

        ~PoolableItem()
        {
            // if the system is shutting down, don't resurrect the object
            if(Environment.HasShutdownStarted || AppDomain.CurrentDomain.IsFinalizingForUnload())
                return;

            // return the object into the pool
            Pool.Return(this as TPoolItem);

            // ensure next time this same finalizer is called again
            GC.ReRegisterForFinalize(this);
        }
    }
}
