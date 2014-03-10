using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.InteropServices;

namespace SuperSocket.SocketBase.Pool
{
    class BufferPool : IntelliPool<byte[]>, IBufferPool
    {
        public int BufferSize { get; private set; }

        public BufferPool(int bufferSize, int initialCount)
            : base(initialCount, new BufferItemCreator(bufferSize))
        {
            BufferSize = bufferSize;
        }
    }
}
