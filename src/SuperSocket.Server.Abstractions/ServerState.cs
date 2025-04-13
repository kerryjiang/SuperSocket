using System;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents the state of a server.
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// The initial state of the server.
        /// </summary>
        None = 0,

        /// <summary>
        /// The server is starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// The server has started.
        /// </summary>
        Started = 2,

        /// <summary>
        /// The server is stopping.
        /// </summary>
        Stopping = 3,

        /// <summary>
        /// The server has stopped.
        /// </summary>
        Stopped = 4,

        /// <summary>
        /// The server failed to start.
        /// </summary>
        Failed = 5
    }
}