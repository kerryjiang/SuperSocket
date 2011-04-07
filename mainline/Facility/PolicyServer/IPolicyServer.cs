using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.Facility.PolicyServer
{
    public interface IPolicyServer
    {
        void ValidateSession(IPolicySession session, SocketAsyncEventArgs e, byte[] data);
    }
}
