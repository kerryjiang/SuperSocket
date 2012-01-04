using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common.Logging
{
    public class Log4NetLog : ILog
    {
        private log4net.ILog m_Log;

        public Log4NetLog(log4net.ILog log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            m_Log = log;
        }

        public bool IsDebugEnabled
        {
            get { return m_Log.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return m_Log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return m_Log.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return m_Log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return m_Log.IsWarnEnabled; }
        }

        public void Debug(object message)
        {
            m_Log.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            m_Log.Debug(message, exception);
        }

        public void DebugFormat(string format, object arg0)
        {
            m_Log.DebugFormat(format, arg0);
        }

        public void DebugFormat(string format, params object[] args)
        {
            m_Log.DebugFormat(format, args);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_Log.DebugFormat(provider, format, args);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            m_Log.DebugFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            m_Log.DebugFormat(format, arg0, arg1, arg2);
        }

        public void Error(object message)
        {
            m_Log.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            m_Log.Error(message, exception);
        }

        public void ErrorFormat(string format, object arg0)
        {
            m_Log.ErrorFormat(format, arg0);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            m_Log.ErrorFormat(format, args);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_Log.ErrorFormat(provider, format, args);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            m_Log.ErrorFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            m_Log.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void Fatal(object message)
        {
            m_Log.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            m_Log.Fatal(message, exception);
        }

        public void FatalFormat(string format, object arg0)
        {
            m_Log.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, params object[] args)
        {
            m_Log.FatalFormat(format, args);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_Log.FatalFormat(provider, format, args);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            m_Log.FatalFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            m_Log.FatalFormat(format, arg0, arg1, arg2);
        }

        public void Info(object message)
        {
            m_Log.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            m_Log.Info(message, exception);
        }

        public void InfoFormat(string format, object arg0)
        {
            m_Log.InfoFormat(format, arg0);
        }

        public void InfoFormat(string format, params object[] args)
        {
            m_Log.InfoFormat(format, args);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_Log.InfoFormat(provider, format, args);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            m_Log.InfoFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            m_Log.InfoFormat(format, arg0, arg1, arg2);
        }

        public void Warn(object message)
        {
            m_Log.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            m_Log.Warn(message, exception);
        }

        public void WarnFormat(string format, object arg0)
        {
            m_Log.WarnFormat(format, arg0);
        }

        public void WarnFormat(string format, params object[] args)
        {
            m_Log.WarnFormat(format, args);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            m_Log.WarnFormat(provider, format, args);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            m_Log.WarnFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            m_Log.WarnFormat(format, arg0, arg1, arg2);
        }
    }
}
