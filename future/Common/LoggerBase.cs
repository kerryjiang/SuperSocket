using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public abstract class LoggerBase : ILogger
    {
        private ILogger m_NestedLogger;

        private LoggerBase()
        {

        }

        public LoggerBase(string name)
            : this(name, null)
        {

        }

        public LoggerBase(string name, ILogger nestedLogger)
        {
            Name = name;
            m_NestedLogger = nestedLogger;
        }

        #region ILogger Members

        public string Name { get; private set; }

        public virtual void LogError(Exception e)
        {
            if (m_NestedLogger != null)
            {
                m_NestedLogger.LogError(e);
            }
        }

        public virtual void LogError(string title, Exception e)
        {
            if (m_NestedLogger != null)
            {
                m_NestedLogger.LogError(title, e);
            }
        }

        public virtual void LogError(string message)
        {
            if (m_NestedLogger != null)
            {
                m_NestedLogger.LogError(message);
            }
        }

        public virtual void LogDebug(string message)
        {
            if (m_NestedLogger != null)
            {
                m_NestedLogger.LogDebug(message);
            }
        }

        public virtual void LogInfo(string message)
        {
            if (m_NestedLogger != null)
            {
                m_NestedLogger.LogInfo(message);
            }
        }

        public virtual void LogPerf(string message)
        {
            if (m_NestedLogger != null)
            {
                m_NestedLogger.LogPerf(message);
            }
        }

        #endregion
    }
}
