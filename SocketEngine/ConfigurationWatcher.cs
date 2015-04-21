using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// The configuration file watcher, it is used for hot configuration updating
    /// </summary>
    public static class ConfigurationWatcher
    {
        private static FileSystemWatcher m_Watcher;

        private static DateTime m_LastUpdatedTime;

        /// <summary>
        /// Watches the specified configuration section.
        /// </summary>
        /// <param name="configSection">The configuration section.</param>
        /// <param name="bootstrap">The bootstrap.</param>
        public static void Watch(ConfigurationSection configSection, IBootstrap bootstrap)
        {
            if (configSection == null)
                throw new ArgumentNullException("configSection");

            if (bootstrap == null)
                throw new ArgumentNullException("bootstrap");

            var sectionName = configSection.SectionInformation.Name;

            var configSourceFile = bootstrap.StartupConfigFile;

            if (string.IsNullOrEmpty(configSourceFile))
                throw new Exception("Cannot get your configuration file's location.");

            m_Watcher = new FileSystemWatcher(Path.GetDirectoryName(configSourceFile), Path.GetFileName(configSourceFile));
            m_Watcher.IncludeSubdirectories = false;
            m_Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            m_Watcher.Changed += (s, e) =>
                {
                    var filePath = e.FullPath;

                    if (!NeedsLoadConfig(filePath))
                        return;

                    lock (m_Watcher)
                    {
                        if (!NeedsLoadConfig(filePath))
                            return;

                        OnConfigFileUpdated(filePath, sectionName, bootstrap);
                        m_LastUpdatedTime = DateTime.Now;
                    }
                };

            m_Watcher.EnableRaisingEvents = true;
        }

        private static bool NeedsLoadConfig(string filePath)
        {
            return File.GetLastWriteTime(filePath) > m_LastUpdatedTime;
        }

        private static void OnConfigFileUpdated(string filePath, string sectionName, IBootstrap bootstrap)
        {
            var fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = filePath;

            System.Configuration.Configuration config;

            try
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            }
            catch (Exception e)
            {
                var loggerProvider = bootstrap as ILoggerProvider;

                if (loggerProvider != null)
                {
                    var logger = loggerProvider.Logger;

                    if (logger != null)
                        logger.Error("Configuraton loading error.", e);
                }

                return;
            }

            var configSource = config.GetSection(sectionName) as IConfigurationSource;

            if (configSource == null)
                return;

            foreach (var serverConfig in configSource.Servers)
            {
                var server = bootstrap.AppServers.FirstOrDefault(x =>
                        x.Name.Equals(serverConfig.Name, StringComparison.OrdinalIgnoreCase));

                if (server == null)
                    continue;

                server.ReportPotentialConfigChange(serverConfig);
            }
        }
    }
}
