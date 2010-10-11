using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public class ConsoleLogger : ILogger
    {
        #region ILogger Members

        public void LogError(Exception e)
        {
            Console.WriteLine(e.Message + " " + e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message + " " + e.InnerException.StackTrace);
            }
        }

        public void LogError(ILogApp app, Exception e)
        {
            Console.WriteLine(app.Name + " : " + e.Message + " " + e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message + " " + e.InnerException.StackTrace);
            }
        }

        public void LogError(string title, Exception e)
        {
            Console.WriteLine(title + " " + e.Message + " " + e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message + " " + e.InnerException.StackTrace);
            }
        }

        public void LogError(ILogApp app, string title, Exception e)
        {
            Console.WriteLine(app.Name + " : " + title + " " + e.Message + " " + e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message + " " + e.InnerException.StackTrace);
            }
        }

        public void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(ILogApp app, string message)
        {
            Console.WriteLine(app.Name + " : " + message);
        }

        public void LogDebug(string message)
        {
            Console.WriteLine(message);
        }

        public void LogDebug(ILogApp app, string message)
        {
            Console.WriteLine(app.Name + " : " + message);
        }

        public void LogInfo(string message)
        {
            Console.WriteLine(message);
        }

        public void LogInfo(ILogApp app, string message)
        {
            Console.WriteLine(app.Name + " : " + message);
        }

        public void LogPerf(string message)
        {
            Console.WriteLine(message);
        }

        public void LogPerf(ILogApp app, string message)
        {
            Console.WriteLine(app.Name + " : " + message);
        }

        #endregion
    }
}
