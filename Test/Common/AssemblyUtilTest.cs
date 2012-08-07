using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;

namespace SuperSocket.Test.Common
{
    [TestFixture]
    public class AssemblyUtilTest
    {
        [Test]
        public void TestCopyProperties()
        {
            var config = new ServerConfig();
            config.Name = "Kerry";
            config.Port = 21;
            var newConfig = new ServerConfig();
            config.CopyPropertiesTo(newConfig);

            Assert.AreEqual(config.Name, newConfig.Name);
            Assert.AreEqual(config.Port, newConfig.Port);
        }
    }
}
