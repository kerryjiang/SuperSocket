using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common.Logging
{
    public static class LogFactoryProvider
    {
        private static ILogFactory m_LogFactory;

        private static volatile bool m_Initialized = false;

        public static void Initialize(ILogFactory logFactory)
        {
            if (m_Initialized)
                throw new Exception("The LogFactoryProvider has been initialized, you cannot initialize it again!");

            m_LogFactory = logFactory;
            GlobalLog = m_LogFactory.GetLog("Global");
            m_Initialized = true;
        }

        public static void Initialize()
        {
            Initialize(new Log4NetLogFactory());
        }

        public static ILogFactory LogFactory
        {
            get { return m_LogFactory; }
        }

        public static ILog GlobalLog { get; private set; }
    }
}
