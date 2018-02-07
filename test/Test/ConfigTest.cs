using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public class ConfigTest
    {

        [Fact]
        public void TestConfigureArgumentExceptions() 
        {
            var server = new SocketServer();

            Assert.Throws<ArgumentNullException>("config",
                () => server.Configure<LinePackageInfo, LinePipelineFilter>(default(ConfigurationRoot)));
        }

        [Fact]
        public void TestConfigure() 
        {
            var server = new SocketServer();

            var dic = new Dictionary<string, string>
            {
                { "name", "TestServer" },
                { "listeners:0:ip", "Any" },
                { "listeners:0:port", "80" },
                { "listeners:0:backLog", "100" },
                { "listeners:1:ip", "Ipv6Any" },
                { "listeners:1:port", "81" }
            };

            var builder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            var config = builder.Build();
            
            Assert.True(server.Configure<LinePackageInfo, LinePipelineFilter>(config));
            Assert.Equal("TestServer", server.Name);

            Assert.Equal(2, server.Listeners.Length);
            Assert.Equal(IPAddress.Any, server.Listeners[0].EndPoint.Address);
            Assert.Equal(80, server.Listeners[0].EndPoint.Port);
            Assert.Equal(IPAddress.IPv6Any, server.Listeners[1].EndPoint.Address);
            Assert.Equal(81, server.Listeners[1].EndPoint.Port);
            Assert.Equal(100, server.Listeners[0].BackLog);
            Assert.Equal(Listener.DefaultBackLog, server.Listeners[1].BackLog);
        }
    }
}
