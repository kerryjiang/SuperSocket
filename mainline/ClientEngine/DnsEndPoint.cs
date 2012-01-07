using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SuperSocket.ClientEngine
{
    public class DnsEndPoint : EndPoint
    {
        public string Host { get; private set; }

        public int Port { get; private set; }

        public DnsEndPoint(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}
