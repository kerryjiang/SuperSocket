using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.SocksServer
{
    class SocksProtocol : ICustomProtocol<BinaryCommandInfo>
    {
        #region ICustomProtocol<BinaryCommandInfo> Members

        public ICommandReader<BinaryCommandInfo> CreateCommandReader(IAppServer appServer)
        {
            return new ConnectCommandReader(appServer);
        }

        #endregion
    }
}
