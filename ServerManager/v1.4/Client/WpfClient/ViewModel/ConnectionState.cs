using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.Client.ViewModel
{
    public enum ConnectionState
    {
        None,
        Connecting,
        Connected,
        NotConnected,
        WaitingReconnect,
        Fault
    }
}
