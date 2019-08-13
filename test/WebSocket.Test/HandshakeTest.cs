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

namespace WebSocket.Test
{
    [Collection("Basic")]
    public class HandshakeTest : TestClassBase
    {
        public HandshakeTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Fact] 
        public async Task TestHandshake() 
        {
            await Task.Delay(0);
        }
    }
}
