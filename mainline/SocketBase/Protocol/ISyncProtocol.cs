using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public interface ISyncProtocol<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        ICommandStreamReader<TCommandInfo> CreateSyncCommandReader();
    }
}
