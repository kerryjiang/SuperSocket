using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents a server with start and stop capabilities.
    /// </summary>
    public interface IServer : IServerInfo, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Starts the server asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        Task<bool> StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the server asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}