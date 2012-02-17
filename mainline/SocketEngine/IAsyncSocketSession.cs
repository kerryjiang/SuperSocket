using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common.Logging;
using SuperSocket.SocketEngine.AsyncSocket;
using System.Net.Sockets;

namespace SuperSocket.SocketEngine
{
    interface IAsyncSocketSessionBase
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; }
        ILog Logger { get; }
        Socket Client { get; }
    }

    interface IAsyncSocketSession : IAsyncSocketSessionBase
    {
        void ProcessReceive(SocketAsyncEventArgs e);
    }
}
