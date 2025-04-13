using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Defines a register for managing connections.
    /// </summary>
    public interface IConnectionRegister
    {
        /// <summary>
        /// Registers a connection asynchronously.
        /// </summary>
        /// <param name="connection">The connection to register.</param>
        /// <returns>A task that represents the asynchronous registration operation.</returns>
        Task RegisterConnection(object connection);
    }
}