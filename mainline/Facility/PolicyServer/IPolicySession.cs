using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.Facility.PolicyServer
{
    public interface IPolicySession
    {
        void Initialize(IPolicyServer server, Socket client, int expectedReceiveLength);
        void StartReceive(SocketAsyncEventArgs e);
        void SendResponse(byte[] response);
        void Close();
    }
}
