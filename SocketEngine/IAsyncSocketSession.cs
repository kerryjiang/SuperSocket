using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Sockets;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    interface IAsyncSocketSessionBase : ILoggerProvider
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; }
        
        ISocket Client { get; }
    }

    interface IAsyncSocketSession : IAsyncSocketSessionBase
    {
        void ProcessReceive(ISocketAsyncEventArgs e);
    }
}
