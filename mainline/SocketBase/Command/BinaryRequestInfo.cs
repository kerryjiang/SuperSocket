using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public class BinaryRequestInfo :  RequestInfo<byte[]>
    {
        public BinaryRequestInfo(string key, byte[] data)
            : base(key, data)
        {

        }
    }
}
