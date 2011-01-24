using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    public interface ISocketServer
    {
        bool Start();
        bool IsRunning { get; }
        void Stop();
    }
}
