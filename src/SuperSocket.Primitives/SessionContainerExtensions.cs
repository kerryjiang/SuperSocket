using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket
{
    public static class SessionContainerExtensions
    {
        public static ISessionContainer ToSyncSessionContainer(this IAsyncSessionContainer asyncSessionContainer)
        {
            return new AsyncToSyncSessionContainerWrapper(asyncSessionContainer);
        }

        public static IAsyncSessionContainer ToAsyncSessionContainer(this ISessionContainer syncSessionContainer)
        {
            return new SyncToAsyncSessionContainerWrapper(syncSessionContainer);
        }

        public static ISessionContainer GetSessionContainer(this IServiceProvider serviceProvider)
        {
            var sessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<ISessionContainer>()
                .FirstOrDefault();

            if (sessionContainer != null)
                return sessionContainer;

            var asyncSessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<IAsyncSessionContainer>()
                .FirstOrDefault();

            return asyncSessionContainer?.ToSyncSessionContainer();
        }

        public static IAsyncSessionContainer GetAsyncSessionContainer(this IServiceProvider serviceProvider)
        {
            var asyncSessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<IAsyncSessionContainer>()
                .FirstOrDefault();

            if (asyncSessionContainer != null)
                return asyncSessionContainer;

            var sessionContainer = serviceProvider.GetServices<IMiddleware>()
                .OfType<ISessionContainer>()
                .FirstOrDefault();

            return sessionContainer?.ToAsyncSessionContainer(); 
        }

        public static ISessionContainer GetSessionContainer(this IServer server)
        {
            return server.ServiceProvider.GetSessionContainer();
        }

        public static IAsyncSessionContainer GetAsyncSessionContainer(this IServer server)
        {
            return server.ServiceProvider.GetAsyncSessionContainer();
        }
    }
}