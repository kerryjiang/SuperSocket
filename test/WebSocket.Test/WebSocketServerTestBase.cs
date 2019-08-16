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
using TestBase;
using SuperSocket.WebSocket.Server;

namespace WebSocket.Test
{
    public abstract class WebSocketServerTestBase : TestClassBase
    {
        public WebSocketServerTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        protected IHostBuilder CreateWebSocketServerBuilder()
        {
            return Configure(WebSocketHostBuilder.Create());
        }
    }
}
