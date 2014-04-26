using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketEngine
{
    sealed class BufferState : PoolableItem<BufferState>
    {
        public byte[] Buffer { get; private set; }

        private IPool<BufferState> m_Pool;

        public BufferState(byte[] buffer, IPool<BufferState> pool)
        {
            Buffer = buffer;
            m_Pool = pool;
        }

        ~BufferState()
        {
            m_Pool.Return(this);
        }
    }
}
