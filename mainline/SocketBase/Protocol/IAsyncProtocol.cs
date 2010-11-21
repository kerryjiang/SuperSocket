using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    public interface IAsyncProtocol<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        ICommandAsyncReader<TCommandInfo> CreateAsyncCommandReader();
    }
}
