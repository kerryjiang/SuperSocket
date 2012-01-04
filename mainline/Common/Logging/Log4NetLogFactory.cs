using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using System.IO;

namespace SuperSocket.Common.Logging
{
    public class Log4NetLogFactory : ILogFactory
    {
        public Log4NetLogFactory()
        {
            if (Path.DirectorySeparatorChar == '\\') // Windows
                Initialize(Path.Combine("Config", "log4net.config"));
            else //Unix
                Initialize(Path.Combine("Config", "log4net.unix.config"));
        }

        public Log4NetLogFactory(string log4netConfig)
        {
            Initialize(log4netConfig);
        }

        private void Initialize(string log4netConfig)
        {
            if (!Path.IsPathRooted(log4netConfig))
                log4netConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, log4netConfig);

            XmlConfigurator.Configure(new FileInfo(log4netConfig));
        }

        public ILog GetLog(string name)
        {
            return new Log4NetLog(LogManager.GetLogger(name));
        }
    }
}
