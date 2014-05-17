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
    class SaeState : BufferBaseState
    {
        public ISocketSession SocketSession { get; internal set; }

        public SocketAsyncEventArgs Sae { get; private set; }

        public SaeState(SocketAsyncEventArgs sae)
        {
            Sae = sae;
        }
    }
}
