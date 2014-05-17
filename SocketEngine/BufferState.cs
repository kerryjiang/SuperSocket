using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Pool;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketEngine
{
    sealed class BufferState : BufferBaseState
    {
        public byte[] Buffer { get; private set; }

        public BufferState(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}
