using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server
{
    public class SessionHandlers
    {
        public Func<IAppSession, ValueTask> Connected { get; set; }

        public Func<IAppSession, CloseEventArgs, ValueTask> Closed { get; set; }
    }
}