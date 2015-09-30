using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.GPSSocketServer
{
    public class GPSSession : AppSession<GPSSession, BufferedPackageInfo>
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
