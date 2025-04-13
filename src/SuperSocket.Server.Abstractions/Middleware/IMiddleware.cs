using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Server.Abstractions.Middleware
{
    /// <summary>
    /// Represents a middleware component in the SuperSocket server pipeline.
    /// </summary>
    public interface IMiddleware
    {
        /// <summary>
        /// Gets the order of the middleware in the pipeline.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Starts the middleware with the specified server.
        /// </summary>
        /// <param name="server">The server instance.</param>
        void Start(IServer server);

        /// <summary>
        /// Shuts down the middleware with the specified server.
        /// </summary>
        /// <param name="server">The server instance.</param>
        void Shutdown(IServer server);

        /// <summary>
        /// Registers a session with the middleware asynchronously.
        /// </summary>
        /// <param name="session">The session to register.</param>
        /// <returns>A task that represents the asynchronous registration operation.</returns>
        ValueTask<bool> RegisterSession(IAppSession session);

        /// <summary>
        /// Unregisters a session from the middleware asynchronously.
        /// </summary>
        /// <param name="session">The session to unregister.</param>
        /// <returns>A task that represents the asynchronous unregistration operation.</returns>
        ValueTask<bool> UnRegisterSession(IAppSession session);
    }
}