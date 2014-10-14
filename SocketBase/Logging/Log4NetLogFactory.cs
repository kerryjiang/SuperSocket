using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;
using log4net.Config;
using log4net.Repository;
using System.Collections.Concurrent;
using log4net.Repository.Hierarchy;

namespace SuperSocket.SocketBase.Logging
{
    /// <summary>
    /// Log4NetLogFactory
    /// </summary>
    public class Log4NetLogFactory : LogFactoryBase
    {
        private string m_ConfigFileName;
        private string m_ConfigFileExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogFactory"/> class.
        /// </summary>
        public Log4NetLogFactory()
            : this("log4net.config")
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogFactory"/> class.
        /// </summary>
        /// <param name="log4netConfig">The log4net config.</param>
        public Log4NetLogFactory(string log4netConfig)
            : base(log4netConfig)
        {

            m_ConfigFileName = Path.GetFileNameWithoutExtension(log4netConfig);
            m_ConfigFileExtension = Path.GetExtension(log4netConfig);

            if (!IsSharedConfig)
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(ConfigFile));
            }
            else
            {
                //Disable Performance logger
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(ConfigFile);
                var docElement = xmlDoc.DocumentElement;
                var perfLogNode = docElement.SelectSingleNode("logger[@name='Performance']");
                if (perfLogNode != null)
                    docElement.RemoveChild(perfLogNode);
                log4net.Config.XmlConfigurator.Configure(docElement);
            }
        }

        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override ILog GetLog(string name)
        {
            return new Log4NetLog(LogManager.GetLogger(name));
        }

        private ConcurrentDictionary<string, Lazy<ILoggerRepository>> m_LoggerRepositories = new ConcurrentDictionary<string, Lazy<ILoggerRepository>>();

        private ConcurrentDictionary<string, ILog> m_LoggersDict = new ConcurrentDictionary<string, ILog>(StringComparer.OrdinalIgnoreCase);

        private Lazy<ILoggerRepository> CreateLazyRepository(string repositoryName)
        {
            var configFile = string.Format("{0}.{1}{2}", m_ConfigFileName, repositoryName, m_ConfigFileExtension);
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", configFile);

            if (!File.Exists(configFilePath))
                return null;

            return new Lazy<ILoggerRepository>(() =>
            {
                var repository = LogManager.CreateRepository(repositoryName);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(repository, new FileInfo(configFilePath));
                return repository;
            });
        }

        private ILoggerRepository EnsureRepository(string repositoryName)
        {
            Lazy<ILoggerRepository> repository;

            if (m_LoggerRepositories.TryGetValue(repositoryName, out repository))
                return repository.Value;

            repository = CreateLazyRepository(repositoryName);

            if (repository == null)
                return null;

            if (m_LoggerRepositories.TryAdd(repositoryName, repository))
                return repository.Value;

            return EnsureRepository(repositoryName);
        }

        /// <summary>
        /// Gets the log from the specific repository.
        /// </summary>
        /// <param name="repositoryName">Name of the repository.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override ILog GetLog(string repositoryName, string name)
        {
            ILoggerRepository repository = EnsureRepository(repositoryName);

            if (repository == null)
                return null;

            var logKey = repositoryName + "-" + name;

            ILog log;

            while (true)
            {
                if (m_LoggersDict.TryGetValue(logKey, out log))
                    return log;

                log = new Log4NetLog(LogManager.GetLogger(repositoryName, name));

                if (m_LoggersDict.TryAdd(logKey, log))
                    return log;
            }
        }
    }
}
