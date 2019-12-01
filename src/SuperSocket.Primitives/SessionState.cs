using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public enum SessionState
    {
        Initialized = 0,

        Connected = 1,

        Closed = 2
    }
}