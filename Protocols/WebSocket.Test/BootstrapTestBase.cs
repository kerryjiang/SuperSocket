using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;

namespace WebSocket.Test
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
                (m_BootStrap as IDisposable).Dispose();
                m_BootStrap = null;
                OnBootstrapCleared();
                GC.Collect();
                GC.WaitForFullGCComplete();
            }
        }

        protected virtual void OnBootstrapCleared()
        {

        }

        protected IConfigurationSource CreateBootstrap(string configFile)
        {
            IBootstrap newBootstrap;
            var configSrc = CreateBootstrap(configFile, out newBootstrap);
            m_BootStrap = newBootstrap;
            return configSrc;
        }

        protected IConfigurationSource CreateBootstrap(string configFile, out IBootstrap newBootstrap)
        {
            var fileMap = new ExeConfigurationFileMap();
            var filePath = Path.Combine(@"Config", configFile);
            fileMap.ExeConfigFilename = filePath;
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var configSource = config.GetSection("superSocket") as IConfigurationSource;

            newBootstrap = BootstrapFactory.CreateBootstrap(configSource);

            return configSource;
        }

        protected IConfigurationSource SetupBootstrap(string configFile, Func<IServerConfig, IServerConfig> configResolver)
        {
            var configSource = CreateBootstrap(configFile);

            if (configResolver != null)
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
