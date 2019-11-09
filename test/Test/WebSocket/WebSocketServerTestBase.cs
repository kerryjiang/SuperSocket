using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.WebSocket.Server;

namespace Tests.WebSocket
{
    public abstract class WebSocketServerTestBase : TestClassBase
    {
        public WebSocketServerTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        protected IHostBuilder CreateWebSocketServerBuilder(Func<IWebSocketHostBuilder,IWebSocketHostBuilder> configurator = null)
        {
            var builder = WebSocketHostBuilder.Create();
            
            if (configurator != null)
                builder = configurator(builder);

            return Configure(builder);
        }
    }
}
