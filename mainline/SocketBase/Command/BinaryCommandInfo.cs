using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public class BinaryCommandInfo : CommandInfoBase<byte[]>
    {
        public BinaryCommandInfo(string key, byte[] data)
            : base(key, data)
        {

        }
    }
}
