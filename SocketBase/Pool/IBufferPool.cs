using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    interface IBufferPool : IPool<byte[]>
    {
        int BufferSize { get; }
    }
}
