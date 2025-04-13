using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions.Middleware
{
    /// <summary>
    /// Provides a base implementation for middleware components in the SuperSocket server pipeline.
    /// </summary>
    public abstract class MiddlewareBase : IMiddleware
    {
        /// <summary>
        /// Gets or sets the order of the middleware in the pipeline.
        /// </summary>
        public int Order { get; protected set; } = 0;

        /// <summary>
        /// Starts the middleware with the specified server.
        /// </summary>
        /// <param name="server">The server instance.</param>
        public virtual void Start(IServer server)
        {

        }

        /// <summary>
        /// Shuts down the middleware with the specified server.
        /// </summary>
        /// <param name="server">The server instance.</param>
        public virtual void Shutdown(IServer server)
        {
            
        }
        
        /// <summary>
        /// Registers a session with the middleware asynchronously.
        /// </summary>
        /// <param name="session">The session to register.</param>
        /// <returns>A task that represents the asynchronous registration operation.</returns>
        public virtual ValueTask<bool> RegisterSession(IAppSession session)
        {
            return new ValueTask<bool>(true);
        }

        /// <summary>
        /// Unregisters a session from the middleware asynchronously.
        /// </summary>
        /// <param name="session">The session to unregister.</param>
        /// <returns>A task that represents the asynchronous unregistration operation.</returns>
        public virtual ValueTask<bool> UnRegisterSession(IAppSession session)
        {
            return new ValueTask<bool>(true);
        }
    }
}