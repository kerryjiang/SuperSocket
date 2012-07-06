using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;

namespace SuperSocket.Common.Logging.EnterpriseLibrary
{
    public class EnterpriseLibraryLog : ILog
    {
        private string m_Category;

        private const string m_MessageTemplate2 = "{0}\r\n{1}";
        private const string m_MessageTemplate3 = "{0}\r\n{1}\r\n{2}";

        private LogWriter m_LogWriter;

        public EnterpriseLibraryLog(LogWriter logWriter, string category)
        {
            m_LogWriter = logWriter;
            m_Category = category;
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        private void Write(object message, string cactegory, TraceEventType logSeverity)
        {
            m_LogWriter.Write(message, cactegory, -1, 1, logSeverity, string.Empty, null);
        }

        public void Debug(object message)
        {
            Write(message, m_Category, System.Diagnostics.TraceEventType.Verbose);
        }

        public void Debug(object message, Exception exception)
        {
            Write(string.Format(m_MessageTemplate3, message, exception.Message, exception.StackTrace), m_Category, TraceEventType.Verbose);
        }

        public void DebugFormat(string format, object arg0)
        {
            Write(string.Format(format, arg0), m_Category, TraceEventType.Verbose);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Write(string.Format(format, args), m_Category, TraceEventType.Verbose);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Write(string.Format(provider, format, args), m_Category, TraceEventType.Verbose);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            Write(string.Format(format, arg0, arg1), m_Category, TraceEventType.Verbose);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Write(string.Format(format, arg0, arg1, arg2), m_Category, TraceEventType.Verbose);
        }

        public void Error(object message)
        {
            Write(message, m_Category, TraceEventType.Error);
        }

        public void Error(object message, Exception exception)
        {
            Write(string.Format(m_MessageTemplate3, message, exception.Message, exception.StackTrace), m_Category, TraceEventType.Error);
        }

        public void ErrorFormat(string format, object arg0)
        {
            Write(string.Format(format, arg0), m_Category, TraceEventType.Error);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Write(string.Format(format, args), m_Category, TraceEventType.Error);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Write(string.Format(provider, format, args), m_Category, TraceEventType.Error);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Write(string.Format(format, arg0, arg1), m_Category, TraceEventType.Error);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Write(string.Format(format, arg0, arg1, arg2), m_Category, TraceEventType.Error);
        }

        public void Fatal(object message)
        {
            Write(message, m_Category, TraceEventType.Critical);
        }

        public void Fatal(object message, Exception exception)
        {
            Write(string.Format(m_MessageTemplate3, message, exception.Message, exception.StackTrace), m_Category, TraceEventType.Critical);
        }

        public void FatalFormat(string format, object arg0)
        {
            Write(string.Format(format, arg0), m_Category, TraceEventType.Critical);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Write(string.Format(format, args), m_Category, TraceEventType.Critical);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Write(string.Format(provider, format, args), m_Category, TraceEventType.Critical);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            Write(string.Format(format, arg0), m_Category, TraceEventType.Critical);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Write(string.Format(format, arg0, arg1, arg2), m_Category, TraceEventType.Critical);
        }

        public void Info(object message)
        {
            Write(message, m_Category, TraceEventType.Information);
        }

        public void Info(object message, Exception exception)
        {
            Write(string.Format(m_MessageTemplate3, message, exception.Message, exception.StackTrace), m_Category, TraceEventType.Information);
        }

        public void InfoFormat(string format, object arg0)
        {
            Write(string.Format(format, arg0), m_Category, TraceEventType.Information);
        }

        public void InfoFormat(string format, params object[] args)
        {
            Write(string.Format(format, args), m_Category, TraceEventType.Information);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Write(string.Format(format, args), m_Category, TraceEventType.Information);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            Write(string.Format(format, arg0, arg1), m_Category, TraceEventType.Information);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Write(string.Format(format, arg0, arg1, arg2), m_Category, TraceEventType.Information);
        }

        public void Warn(object message)
        {
            Write(message, m_Category, TraceEventType.Warning);
        }

        public void Warn(object message, Exception exception)
        {
            Write(string.Format(m_MessageTemplate3, message, exception.Message, exception.StackTrace), m_Category, TraceEventType.Warning);
        }

        public void WarnFormat(string format, object arg0)
        {
            Write(string.Format(format, arg0), m_Category, TraceEventType.Warning);
        }

        public void WarnFormat(string format, params object[] args)
        {
            Write(string.Format(format, args), m_Category, TraceEventType.Warning);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            Write(string.Format(provider, format, args), m_Category, TraceEventType.Warning);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            Write(string.Format(format, arg0, arg1), m_Category, TraceEventType.Warning);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Write(string.Format(format, arg0, arg1, arg2), m_Category, TraceEventType.Warning);
        }
    }
}
