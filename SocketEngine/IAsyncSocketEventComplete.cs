using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.SocketEngine
{
    interface IAsyncSocketEventComplete
    {
        void HandleSocketEventComplete(object sender, SocketAsyncEventArgs e);
    }
}
