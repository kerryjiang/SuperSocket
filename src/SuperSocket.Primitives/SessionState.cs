using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public enum SessionState
    {
        None = 0,

        Initialized = 1,

        Connected = 2,

        Closed = 3
    }
}