using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Server.Abstractions.Session
{
    /// <summary>
    /// Represents a host for handling session-related events.
    /// </summary>
    public interface ISessionEventHost
    {
        /// <summary>
        /// Handles the event when a session is connected.
        /// </summary>
        /// <param name="session">The session that has connected.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask HandleSessionConnectedEvent(IAppSession session);

        /// <summary>
        /// Handles the event when a session is closed.
        /// </summary>
        /// <param name="session">The session that has closed.</param>
        /// <param name="reason">The reason for closing the session.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask HandleSessionClosedEvent(IAppSession session, CloseReason reason);
    }
}