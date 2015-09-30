using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    public class GPSServer : AppServer<GPSSession, BufferedPackageInfo>
    {
        public GPSServer()
            : base(new DefaultReceiveFilterFactory<GPSReceiveFilter, BufferedPackageInfo>())
        {
            DefaultResponse = new byte[] { 0x54, 0x68, 0x1a, 0x0d, 0x0a};
        }

        internal byte[] DefaultResponse { get; private set; }
    }
}
