using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Defines a factory for creating connection stream initializers.
    /// </summary>
    public interface IConnectionStreamInitializersFactory
    {
        /// <summary>
        /// Creates a collection of connection stream initializers based on the specified listen options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        /// <returns>A collection of connection stream initializers.</returns>
        IEnumerable<IConnectionStreamInitializer> Create(ListenOptions listenOptions);
    }
}