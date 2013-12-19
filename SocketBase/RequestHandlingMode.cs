using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Request handling mode
    /// </summary>
    public enum RequestHandlingMode
    {
        /// <summary>
        /// The system working thread pool, default option
        /// </summary>
        Pool,
        /// <summary>
        /// Single thread
        /// </summary>
        SingleThread,

        /// <summary>
        /// Multiple dedicated threads
        /// </summary>
        MultipleThread
    }
}
