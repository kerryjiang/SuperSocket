using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Configuration;
using System.IO;
using SuperSocket.SocketEngine.Configuration;
using SuperSocket.SocketEngine;

namespace SuperSocket.Test
{
    [TestFixture]
    public class CustomChildConfigTest
    {
        private ConfigurationSection GetConfigSection(string configFile)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = Path.Combine(@"Config", configFile);

            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            return config.GetSection("socketServer");
        }

        [Test]
        public void TestCustomChildConfig()
        {
            var configSection = GetConfigSection("ChildConfig.config") as SocketServiceConfig;

            var appServer = new ChildrenConfigTestServer();

            Assert.IsTrue(appServer.Setup(configSection, configSection.Servers.FirstOrDefault(), SocketServerFactory.Instance));

            Assert.AreEqual(168, appServer.ChildConfigValue);
            Assert.AreEqual(8848, appServer.ChildConfigGlobalValue);
            Assert.AreEqual(10 + 20 + 30, appServer.ChildConfigValueSum);
            Assert.AreEqual(10 * 20 * 30, appServer.ChildConfigValueMultiplyProduct);
        }
    }
}
