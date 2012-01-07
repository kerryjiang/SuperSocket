using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.ClientEngine
{
    class ConnectStateToken
    {
        public IPAddress[] Addresses { get; set; }

        public int CurrentConnectIndex { get; set; }

        public int Port { get; set; }

        public Socket Socket { get; set; }
    }
}
