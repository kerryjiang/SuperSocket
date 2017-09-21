using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using System.IO.Pipelines.Networking.Libuv;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Libuv;

namespace SuperSocket
{
    public static class Extensions
    {
        public static void UseLibuvListener(IServiceCollection services)
        {
            services.AddTransient<IPipeConnectionListener, LibuvPipeConnectionListener>();
        }
    }
}