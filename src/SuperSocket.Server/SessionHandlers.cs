using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class SessionHandlers
    {
        public Func<IAppSession, ValueTask> Connected { get; set; }

        public Func<IAppSession, CloseEventArgs, ValueTask> Closed { get; set; }
    }
}