using Microsoft.Extensions.DependencyInjection;
using SuperSocket.NetSocket;

namespace SuperSocket
{
    public static class Extensions
    {
        public static void UseNetSocketListener(this IServiceCollection services)
        {
            services.AddTransient<IPipeConnectionListener, NetSocketPipeConnectionListener>();
        }
    }
}