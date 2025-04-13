using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents the state of a session.
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// The session is in its initial state.
        /// </summary>
        None = 0,

        /// <summary>
        /// The session has been initialized.
        /// </summary>
        Initialized = 1,

        /// <summary>
        /// The session is connected.
        /// </summary>
        Connected = 2,

        /// <summary>
        /// The session has been closed.
        /// </summary>
        Closed = 3
    }
}