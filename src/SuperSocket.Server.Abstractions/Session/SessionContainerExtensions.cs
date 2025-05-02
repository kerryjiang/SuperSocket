using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server.Abstractions.Middleware;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Provides extension methods for session container operations.
    /// </summary>
    public static class SessionContainerExtensions
    {
        /// <summary>
        /// Converts an asynchronous session container to a synchronous session container.
        /// </summary>
        /// <param name="asyncSessionContainer">The asynchronous session container to convert.</param>
        /// <returns>A synchronous session container that wraps the asynchronous operations.</returns>
        public static ISessionContainer ToSyncSessionContainer(this IAsyncSessionContainer asyncSessionContainer)
        {
            return new AsyncToSyncSessionContainerWrapper(asyncSessionContainer);
        }

        /// <summary>
        /// Converts a synchronous session container to an asynchronous session container.
        /// </summary>
        /// <param name="syncSessionContainer">The synchronous session container to convert.</param>
        /// <returns>An asynchronous session container that wraps the synchronous operations.</returns>
        public static IAsyncSessionContainer ToAsyncSessionContainer(this ISessionContainer syncSessionContainer)
        {
            return new SyncToAsyncSessionContainerWrapper(syncSessionContainer);
        }

        /// <summary>
        /// Gets a session container from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A session container if available; otherwise, null.</returns>
        [Obsolete("Please use the method server.GetSessionContainer() instead.")]
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

        /// <summary>
        /// Gets an asynchronous session container from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>An asynchronous session container if available; otherwise, null.</returns>
        [Obsolete("Please use the method server.GetSessionContainer() instead.")]
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

        /// <summary>
        /// Gets a session container from the server.
        /// </summary>
        /// <param name="server">The server information.</param>
        /// <returns>A session container if available; otherwise, null.</returns>
        public static ISessionContainer GetSessionContainer(this IServerInfo server)
        {
            #pragma warning disable CS0618
            return server.ServiceProvider.GetSessionContainer();
            #pragma warning restore CS0618
        }

        /// <summary>
        /// Gets an asynchronous session container from the server.
        /// </summary>
        /// <param name="server">The server information.</param>
        /// <returns>An asynchronous session container if available; otherwise, null.</returns>
        public static IAsyncSessionContainer GetAsyncSessionContainer(this IServerInfo server)
        {
            #pragma warning disable CS0618
            return server.ServiceProvider.GetAsyncSessionContainer();
            #pragma warning restore CS0618
        }
    }
}