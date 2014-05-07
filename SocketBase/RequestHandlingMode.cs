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
        /// The system IOCP thread pool, the request will be handled by the same IO thread which received the data, default option
        /// </summary>
        Default,
        /// <summary>
        /// Single thread
        /// </summary>
        SingleThread,

        /// <summary>
        /// The SuperSocket customized thread pool to ensure first received requests must be handled first
        /// </summary>
        CustomThreadPool
    }
}
