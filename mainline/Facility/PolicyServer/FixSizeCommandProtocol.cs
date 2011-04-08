using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    public class FixSizeCommandProtocol : ICustomProtocol<BinaryCommandInfo>
    {
        public int FixCommandSize { get; private set; }

        public FixSizeCommandProtocol(int fixCommandSize)
        {
            FixCommandSize = fixCommandSize;
        }

        public ICommandReader<BinaryCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new FixSizeCommandReader(appServer, FixCommandSize);
        }
    }
}
