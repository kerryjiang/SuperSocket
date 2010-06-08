using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore;

namespace RemoteProcessService
{
    public class RemoteProcessServer : AppServer<RemotePrcessSession>
    {
        public override bool IsReady
        {
            get { return true; }
        }
    }
}
