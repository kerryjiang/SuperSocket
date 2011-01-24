using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public class ConsoleLogger : ILogger
    {
        public string Name { get; private set; }

        public ConsoleLogger()
            : this("SuperSocket")
        {

        }

        public ConsoleLogger(string name)
        {
            Name = name;
        }

        #region ILogger Members

        public void LogError(Exception e)
        {
            Console.WriteLine(Name + " : " + e.Message + " " + e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message + " " + e.InnerException.StackTrace);
            }
        }

        public void LogError(string title, Exception e)
        {
            Console.WriteLine(Name + " : " + title + " " + e.Message + " " + e.StackTrace);
            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message + " " + e.InnerException.StackTrace);
            }
        }

        public void LogError(string message)
        {
            Console.WriteLine(Name + " : " + message);
        }

        public void LogDebug(string message)
        {
            Console.WriteLine(Name + " : " + message);
        }

        public void LogInfo(string message)
        {
            Console.WriteLine(Name + " : " + message);
        }

        public void LogPerf(string message)
        {
            Console.WriteLine(Name + " : " + message);
        }

        #endregion
    }
}
