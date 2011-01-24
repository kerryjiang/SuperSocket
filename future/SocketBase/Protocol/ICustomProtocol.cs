using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Protocol
{
    public interface ICustomProtocol<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        ICommandReader<TCommandInfo> CreateCommandReader(IAppServer appServer);
    }
}
