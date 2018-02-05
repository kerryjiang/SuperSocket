using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.NetSocket;

namespace SuperSocket
{
    public static class Extensions
    {
        public static void UseNetSocketListener(this IServiceCollection services)
        {
            services.AddTransient<IDuplexPipeListener, NetSocketPipeListener>();
        }
    }
}