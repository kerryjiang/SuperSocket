using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common.Logging
{
    /// <summary>
    /// LogFactoryProvider
    /// </summary>
    public static class LogFactoryProvider
    {
        private static ILogFactory m_LogFactory;

        private static volatile bool m_Initialized = false;

        /// <summary>
        /// Initializes the specified log factory.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        public static void Initialize(ILogFactory logFactory)
        {
            if (m_Initialized)
                throw new Exception("The LogFactoryProvider has been initialized, you cannot initialize it again!");

            m_LogFactory = logFactory;
            GlobalLog = m_LogFactory.GetLog("Global");
            m_Initialized = true;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            Initialize(new Log4NetLogFactory());
        }

        /// <summary>
        /// Gets the log factory.
        /// </summary>
        public static ILogFactory LogFactory
        {
            get { return m_LogFactory; }
        }

        /// <summary>
        /// Gets the global log.
        /// </summary>
        public static ILog GlobalLog { get; private set; }
    }
}
