using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.SocksServer
{
    public class SocksSocketContext : SocketContext
    {
        public int SocksVersion { get; set; }
        public int CommandCode { get; set; }
        public string UserID { get; set; }
    }
}
