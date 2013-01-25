using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase;
using System.Configuration;
using System.IO;

namespace SuperSocket.Test
{
    public abstract class BootstrapTestBase
    {
        private IBootstrap m_BootStrap;

        protected IBootstrap BootStrap
        {
            get { return m_BootStrap; }
        }

        [TearDown]
        public void ClearBootstrap()
        {
            if (m_BootStrap != null)
            {
                m_BootStrap.Stop();
                m_BootStrap = null;
                OnBootstrapCleared();
            }
        }

        protected virtual void OnBootstrapCleared()
        {

        }

        protected IConfigurationSource CreateBootstrap(string configFile)
        {
            var fileMap = new ExeConfigurationFileMap();
            var filePath = Path.Combine(@"Config", configFile);
            fileMap.ExeConfigFilename = filePath;
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var configSource = config.GetSection("superSocket") as IConfigurationSource;

            m_BootStrap = BootstrapFactory.CreateBootstrap(configSource);

            return configSource;
        }

        protected IConfigurationSource SetupBootstrap(string configFile, Func<IServerConfig, IServerConfig> configResolver)
        {
            var configSource = CreateBootstrap(configFile);

            if(configResolver != null)
                Assert.IsTrue(m_BootStrap.Initialize(configResolver));
            else
                Assert.IsTrue(m_BootStrap.Initialize());

            return configSource;
        }

        protected IConfigurationSource SetupBootstrap(string configFile)
        {
            return SetupBootstrap(configFile, null);
        }

        protected IConfigurationSource StartBootstrap(string configFile, Func<IServerConfig, IServerConfig> configResolver)
        {
            var configSource = SetupBootstrap(configFile, configResolver);
            var result = m_BootStrap.Start();
            Assert.AreEqual(StartResult.Success, result);
            return configSource;
        }

        protected IConfigurationSource StartBootstrap(string configFile)
        {
            return StartBootstrap(configFile, null);
        }
    }
}
