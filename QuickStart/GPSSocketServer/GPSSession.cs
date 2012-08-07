using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    public class GPSSession : AppSession<GPSSession, BinaryRequestInfo>
    {
        public new GPSServer AppServer
        {
            get
            {
                return (GPSServer)base.AppServer;
            }
        }
    }
}
