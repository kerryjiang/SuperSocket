using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using System.Net.Sockets;
using System.Threading;
using SuperSocket.SocketBase.Pool;

namespace SuperSocket.SocketEngine
{
    class SaeState
    {
        public ISocketSession SocketSession { get; internal set; }

        public SocketAsyncEventArgs Sae { get; private set; }

        private IPool<SocketAsyncEventArgs> m_Pool;

        public SaeState(SocketAsyncEventArgs sae, IPool<SocketAsyncEventArgs> pool)
        {
            Sae = sae;
            m_Pool = pool;
        }

        ~SaeState()
        {
            //Clean the assosiated session before the SAE is returned to the pool
            SocketSession = null;
            m_Pool.Return(Sae);
        }
    }
}
