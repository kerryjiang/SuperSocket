using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    public enum SessionState
    {
        None = 0,

        Initialized = 1,

        Connected = 2,

        Closed = 3
    }
}