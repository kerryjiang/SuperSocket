using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common.Logging;

namespace SuperSocket.Common
{
    /// <summary>
    /// The interface for who provides logger
    /// </summary>
    public interface ILoggerProvider
    {
        /// <summary>
        /// Gets the logger assosiated with this object.
        /// </summary>
        ILog Logger { get; }
    }
}
