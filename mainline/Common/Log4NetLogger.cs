using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Core;

namespace SuperSocket.Common
{
    public class Log4NetLogger : LoggerBase
    {
        protected ILog Logger { get; private set; }

        private string m_ServerTemplate = "{0}:";
        private string m_ServerDetailTemplate = "{0} : {1}";

        public Log4NetLogger()
            : this("SuperSocket")
        {

        }

        public Log4NetLogger(string name)
            : this(name, null)
        {
            
        }

        public Log4NetLogger(string name, ILogger nestedLogger)
            : base(name, nestedLogger)
        {
            Logger = LogManager.GetLogger(name);
        }

        #region ILogger Members

        public override void LogError(Exception e)
        {
            Logger.Error(string.Format(m_ServerTemplate, Name), e);
            base.LogError(e);
        }

        public override void LogError(string title, Exception e)
        {
            Logger.Error(string.Format(m_ServerDetailTemplate, Name, title), e);
            base.LogError(title, e);
        }

        public override void LogError(string message)
        {
            Logger.ErrorFormat(m_ServerDetailTemplate, Name, message);
            base.LogError(message);
        }

        public override void LogDebug(string message)
        {
            Logger.DebugFormat(m_ServerDetailTemplate, Name, message);
            base.LogDebug(message);
        }

        public override void LogInfo(string message)
        {
            Logger.InfoFormat(m_ServerDetailTemplate, Name, message);
            base.LogInfo(message);
        }

        public override void LogPerf(string message)
        {
            Logger.Logger.Log(new LoggingEvent(new LoggingEventData
                {
                    LoggerName = Name,
                    Message = message,
                    Level = Level.Trace
                }));
            base.LogPerf(message);
        }

        #endregion
    }
}
