using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Represents a session that requires a handshake.
    /// </summary>
    public interface IHandshakeRequiredSession
    {
        /// <summary>
        /// Gets a value indicating whether the handshake has been completed.
        /// </summary>
        bool Handshaked { get; }
    }
}