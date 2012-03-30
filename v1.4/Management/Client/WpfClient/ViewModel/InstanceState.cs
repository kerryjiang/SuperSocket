using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Client.ViewModel
{
    public enum InstanceState
    {
        Connecting,
        NotStarted,
        Running,
        Starting,
        Stopping,
        NotConnected,
    }
}
