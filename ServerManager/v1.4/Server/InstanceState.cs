using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.Management.Server
{
    public class InstanceState
    {
        public IAppServer Instance { get; set; }

        public PerformanceData Performance { get; set; }
    }
}
