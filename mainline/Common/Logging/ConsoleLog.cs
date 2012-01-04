using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common.Logging
{
    public class ConsoleLog : ILog
    {
        private string m_Name;

        private const string m_MessageTemplate = "{0}-{1}: {2}";

        private const string m_Debug = "DEBUG";

        private const string m_Error = "ERROR";

        private const string m_Fatal = "FATAL";

        private const string m_Info = "INFO";

        private const string m_Warn = "WARN";

        public ConsoleLog(string name)
        {
            m_Name = name;
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

        public void Debug(object message)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, message);
        }

        public void Debug(object message, Exception exception)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, message + Environment.NewLine + exception.Message + Environment.StackTrace);
        }

        public void DebugFormat(string format, object arg0)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, string.Format(format, arg0));
        }

        public void DebugFormat(string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, string.Format(format, args));
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, string.Format(provider, format, args));
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, string.Format(format, arg0, arg1));
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Debug, string.Format(format, arg0, arg1, arg2));
        }

        public void Error(object message)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, message);
        }

        public void Error(object message, Exception exception)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, message + Environment.NewLine + exception.Message + Environment.StackTrace);
        }

        public void ErrorFormat(string format, object arg0)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, string.Format(format, arg0));
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, string.Format(format, args));
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, string.Format(provider, format, args));
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, string.Format(format, arg0, arg1));
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Error, string.Format(format, arg0, arg2));
        }

        public void Fatal(object message)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, message);
        }

        public void Fatal(object message, Exception exception)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, message + Environment.NewLine + exception.Message + Environment.StackTrace);
        }

        public void FatalFormat(string format, object arg0)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, string.Format(format, arg0));
        }

        public void FatalFormat(string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, string.Format(format, args));
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, string.Format(provider, format, args));
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, string.Format(format, arg0, arg1));
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Fatal, string.Format(format, arg0, arg1, arg2));
        }

        public void Info(object message)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, message);
        }

        public void Info(object message, Exception exception)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, message + Environment.NewLine + exception.Message + Environment.StackTrace);
        }

        public void InfoFormat(string format, object arg0)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, string.Format(format, arg0));
        }

        public void InfoFormat(string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, string.Format(format, args));
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, string.Format(provider, format, args));
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, string.Format(format, arg0, arg1));
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Info, string.Format(format, arg0, arg1, arg2));
        }

        public void Warn(object message)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, message);
        }

        public void Warn(object message, Exception exception)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, message + Environment.NewLine + exception.Message + Environment.StackTrace);
        }

        public void WarnFormat(string format, object arg0)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, string.Format(format, arg0));
        }

        public void WarnFormat(string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, string.Format(format, args));
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, string.Format(provider, format, args));
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, string.Format(format, arg0, arg1));
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(m_MessageTemplate, m_Name, m_Warn, string.Format(format, arg0, arg1, arg2));
        }
    }
}
