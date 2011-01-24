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
            Logger.Error(e);
            base.LogError(e);
        }

        public override void LogError(string title, Exception e)
        {
            Logger.Error(title, e);
            base.LogError(title, e);
        }

        public override void LogError(string message)
        {
            Logger.Error(message);
            base.LogError(message);
        }

        public override void LogDebug(string message)
        {
            Logger.Debug(message);
            base.LogDebug(message);
        }

        public override void LogInfo(string message)
        {
            Logger.Info(message);
            base.LogInfo(message);
        }

        public override void LogPerf(string message)
        {
            Logger.Logger.Log(new LoggingEvent(new LoggingEventData
                {
                    LoggerName = Name,
                    Message = message,
                    Level = Level.Trace,
                    TimeStamp = DateTime.Now,
                    Domain = AppDomain.CurrentDomain.FriendlyName
                }));
            base.LogPerf(message);
        }

        #endregion
    }
}
