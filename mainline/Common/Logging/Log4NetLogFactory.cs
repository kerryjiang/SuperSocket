using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using System.IO;

namespace SuperSocket.Common.Logging
{
    /// <summary>
    /// Log4NetLogFactory
    /// </summary>
    public class Log4NetLogFactory : ILogFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogFactory"/> class.
        /// </summary>
        public Log4NetLogFactory()
        {
            if (Path.DirectorySeparatorChar == '\\') // Windows
                Initialize(Path.Combine("Config", "log4net.config"));
            else //Unix
                Initialize(Path.Combine("Config", "log4net.unix.config"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogFactory"/> class.
        /// </summary>
        /// <param name="log4netConfig">The log4net config.</param>
        public Log4NetLogFactory(string log4netConfig)
        {
            Initialize(log4netConfig);
        }

        /// <summary>
        /// Initializes the specified log4net config.
        /// </summary>
        /// <param name="log4netConfig">The log4net config.</param>
        private void Initialize(string log4netConfig)
        {
            if (!Path.IsPathRooted(log4netConfig))
                log4netConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, log4netConfig);

            XmlConfigurator.Configure(new FileInfo(log4netConfig));
        }

        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ILog GetLog(string name)
        {
            return new Log4NetLog(LogManager.GetLogger(name));
        }
    }
}
