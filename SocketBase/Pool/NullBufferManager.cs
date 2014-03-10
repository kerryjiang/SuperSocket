using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Pool
{
    class NullBufferManager : IBufferManager
    {
        public byte[] GetBuffer(int size)
        {
            return new byte[size];
        }

        public void ReturnBuffer(byte[] buffer)
        {
            //Do nothing
        }

        public void Shrink()
        {
            //Do nothing
        }
    }
}
