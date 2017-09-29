using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public abstract class TestBase
    {
        protected abstract void RegisterServices(IServiceCollection services);

        [Fact]
        public async Task TestSessionCount() 
        {
            var server = new SocketServer();

            var dic = new Dictionary<string, string>
            {
                { "name", "TestServer" },
                { "listeners:0:ip", "Any" },
                { "listeners:0:port", "4040" }
            };

            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            var config = builder.Build();

            RegisterServices(services);
            
            Assert.True(server.Configure<FakePackageInfo, FakePipelineFilter>(config, services));
            Assert.Equal("TestServer", server.Name);

            Assert.True(server.Start());
            Assert.Equal(0, server.SessionCount);

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));

            await Task.Delay(1);

            Assert.Equal(1, server.SessionCount);

            server.Stop();
        }
    }
}
