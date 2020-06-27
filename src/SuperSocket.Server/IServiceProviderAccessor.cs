using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    interface IServiceProviderAccessor
    {
        IServiceProvider ServiceProvider { get; }
    }
}