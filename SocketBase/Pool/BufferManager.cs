using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Pool
{
    /// <summary>
    /// The default implemented BufferManager
    /// </summary>
    public class BufferManager : IBufferManager
    {
        private const int m_Magic = 4096;//4k

        private IBufferPool[] m_Pools;

        private IBufferPool m_LastHitPool;

        //4096 = 4k, 8192 = 8k, 16384 = 16k
        private static BufferPoolConfig[] m_DefaultDefinition = new BufferPoolConfig[]
            {   new BufferPoolConfig(4096, 200),
                new BufferPoolConfig(8192, 50),
                new BufferPoolConfig(16384, 10)
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferManager"/> class.
        /// </summary>
        public BufferManager()
            : this(m_DefaultDefinition)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferManager"/> class.
        /// </summary>
        /// <param name="defintion">The defintion.</param>
        public BufferManager(IBufferPoolConfig[] defintion)
        {
            m_Pools = (defintion != null && defintion.Length > 0) ?
                defintion.Select(d => new BufferPool(d.BufferSize, d.InitialCount)).ToArray()
                : m_DefaultDefinition.Select(d => new BufferPool(d.BufferSize, d.InitialCount)).ToArray();
        }

        /// <summary>
        /// Gets a buffer of the specific size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public byte[] GetBuffer(int size)
        {
            var lastHitPool = m_LastHitPool;

            if (lastHitPool != null && lastHitPool.BufferSize >= size)
                return lastHitPool.Get();

            for(var i = 0; i < m_Pools.Length; i++)
            {
                var pool = m_Pools[i];

                if(pool.BufferSize >= size)
                {
                    m_LastHitPool = pool;
                    return pool.Get();
                }
            }

            //The size is large than any pool's buffer size, so create the buffer directly now
            return new byte[size];
        }

        /// <summary>
        /// Returns the buffer to pool.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void ReturnBuffer(byte[] buffer)
        {
            var size = buffer.Length;

            var lastHitPool = m_LastHitPool;

            if (lastHitPool != null && lastHitPool.BufferSize == size)
            {
                lastHitPool.Return(buffer);
                return;
            }

            for(var i = 0; i < m_Pools.Length; i++)
            {
                var pool = m_Pools[i];

                if(pool.BufferSize == size)
                {
                    m_LastHitPool = pool;
                    pool.Return(buffer);
                    return;
                }
            }
        }

        /// <summary>
        /// Shrinks this size of pools.
        /// </summary>
        public void Shrink()
        {
            for (var i = 0; i < m_Pools.Length; i++)
            {
                m_Pools[i].Shrink();
            }
        }
    }
}
