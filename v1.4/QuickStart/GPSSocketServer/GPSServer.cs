using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    public class GPSServer : AppServer<GPSSession, BinaryCommandInfo>
    {
        public GPSServer()
            : base(new GPSCustomProtocol())
        {
            DefaultResponse = new byte[] { 0x54, 0x68, 0x1a, 0x0d, 0x0a};
        }

        internal byte[] DefaultResponse { get; private set; }
    }
}
