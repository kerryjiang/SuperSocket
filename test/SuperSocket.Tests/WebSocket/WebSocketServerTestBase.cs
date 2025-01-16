using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using Xunit;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.WebSocket.Server;
using SuperSocket.WebSocket;
using SuperSocket.Server.Abstractions.Host;

namespace SuperSocket.Tests.WebSocket
{
    public abstract class WebSocketServerTestBase : TestClassBase
    {
        public WebSocketServerTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        protected ISuperSocketHostBuilder<WebSocketPackage> CreateWebSocketServerBuilder(Func<ISuperSocketHostBuilder<WebSocketPackage>, ISuperSocketHostBuilder<WebSocketPackage>> configurator = null, IHostConfigurator hostConfigurator = null)
        {
            ISuperSocketHostBuilder<WebSocketPackage> builder = WebSocketHostBuilder.Create();
            
            if (configurator != null)
                builder = configurator(builder);

            return Configure(builder, hostConfigurator) as ISuperSocketHostBuilder<WebSocketPackage>;
        }
    }
}
