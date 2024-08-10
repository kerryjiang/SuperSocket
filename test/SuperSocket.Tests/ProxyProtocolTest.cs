using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Host;
using SuperSocket.Server.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace SuperSocket.Tests
{
    [Trait("Category", "ProxyProtocol")]
    public class ProxyProtocolTest : FixedHeaderProtocolTest
    {
        public ProxyProtocolTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override IHostConfigurator CreateHostConfigurator(Type hostConfiguratorType)
        {
            return new ProxyProtocolHostConfigurator(base.CreateHostConfigurator(hostConfiguratorType));
        }

        protected override Dictionary<string, string> LoadMemoryConfig(Dictionary<string, string> configSettings)
        {
            base.LoadMemoryConfig(configSettings);
            configSettings["serverOptions:enableProxyProtocol"] = "true";
            return configSettings;
        }
    }
}
