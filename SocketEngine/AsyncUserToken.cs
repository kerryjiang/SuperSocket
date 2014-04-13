using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using System.Net.Sockets;
using System.Threading;

namespace SuperSocket.SocketEngine
{
    class AsyncUserToken : ISegmentState
    {
        private int m_ReferenceCount;

        public int ReferenceCount
        {
            get { return m_ReferenceCount; }
        }

        public void IncreaseReference()
        {
            Interlocked.Increment(ref m_ReferenceCount);
        }

        public int DecreaseReference()
        {
            return Interlocked.Decrement(ref m_ReferenceCount);
        }

        public ISocketSession SocketSession { get; internal set; }

        public SocketAsyncEventArgs SAE { get; internal set; }
    }
}
