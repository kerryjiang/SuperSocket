using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.Internal;
using SuperSocket;


namespace Tests
{
    public class NetSocketTest : TestBase
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITransportFactory, SocketTransportFactory>();
        }
    }
}
