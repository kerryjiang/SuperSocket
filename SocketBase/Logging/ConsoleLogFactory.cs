using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Logging
{
    /// <summary>
    /// Console log factory
    /// </summary>
    public class ConsoleLogFactory : ILogFactory
    {
        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ILog GetLog(string name)
        {
            return new ConsoleLog(name);
        }
    }
}
