using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The processing state
    /// </summary>
    public enum ProcessState : byte
    {
        /// <summary>
        /// The being processed data was processed completely
        /// </summary>
        Completed,
        /// <summary>
        /// The being processed data was cached
        /// </summary>
        Cached,
        /// <summary>
        /// The processor is in error state
        /// </summary>
        Error
    }
}
