using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public class BinaryCommandInfo : CommandInfo<byte[]>
    {
        public BinaryCommandInfo(string key, byte[] data)
            : base(key, data)
        {

        }
    }
}
