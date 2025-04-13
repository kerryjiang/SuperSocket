using System;
using System.Net.Sockets;

namespace SuperSocket.Server.Abstractions.Connections
{
    /// <summary>
    /// Represents a setter for configuring socket options.
    /// </summary>
    public class SocketOptionsSetter
    {
        /// <summary>
        /// Gets the action to set socket options.
        /// </summary>
        public Action<Socket> Setter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketOptionsSetter"/> class.
        /// </summary>
        /// <param name="setter">The action to set socket options.</param>
        public SocketOptionsSetter(Action<Socket> setter)
        {
            Setter = setter;
        }
    }
}