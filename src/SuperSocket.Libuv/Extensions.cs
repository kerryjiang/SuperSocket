using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Libuv;

namespace SuperSocket
{
    public static class Extensions
    {
        public static void UseLibuvListener(this IServiceCollection services)
        {
            services.AddTransient<IPipeConnectionListener, LibuvPipeConnectionListener>();
        }
    }
}