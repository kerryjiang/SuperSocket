using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void TestConfigureArgumentExceptions() 
        {
            var server = new SocketServer();

            Assert.Throws<ArgumentNullException>("config", 
                () => server.Configure(default(IConfiguration)));

            Assert.Throws<ArgumentNullException>("services", 
                () => server.Configure(default(IServiceCollection), default(IConfiguration)));
        }

        [Fact]
        public void TestConfigure() 
        {
            var server = new SocketServer();

            var dic = new Dictionary<string, string>
            {
                { "Name", "TestServer" } ,
            };

            var builder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            var config = builder.Build();
            
            Assert.True(server.Configure(config));
            Assert.Equal("TestServer", server.Name);
        }
    }
}
