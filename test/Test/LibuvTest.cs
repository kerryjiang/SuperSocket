using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal;
using SuperSocket;


namespace Tests
{
    public class LibuvTest : TestBase
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITransportFactory, LibuvTransportFactory>();
        }
    }
}
