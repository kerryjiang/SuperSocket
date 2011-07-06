using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    public class GPSSession : AppSession<GPSSession, BinaryCommandInfo>
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
