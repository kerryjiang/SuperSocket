using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using Xunit;

namespace Tests
{
    public class MainTest : TestBase
    {

        [Fact] 
        public async Task TestSessionCount() 
        {
            var server = CreateSocketServer<TextPackageInfo, LinePipelineFilter>(packageHandler: async (s, p) =>
            {
                await s.Channel.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(p.Text + "\r\n")));
            });

            Assert.Equal("TestServer", server.Name);

            Assert.True(await server.StartAsync());
            Console.WriteLine("Started.");

            Assert.Equal(0, server.SessionCount);
            Console.WriteLine("SessionCount:" + server.SessionCount);

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
            Console.WriteLine("Connected.");

            await Task.Delay(3000);

            Assert.Equal(1, server.SessionCount);
            Console.WriteLine("SessionCount:" + server.SessionCount);

            client.Shutdown(SocketShutdown.Both);
            client.Close();

            await Task.Delay(1000);

            Assert.Equal(0, server.SessionCount);
            Console.WriteLine("SessionCount:" + server.SessionCount);

            await server.StopAsync();
        }

        [Fact]
        public async Task TestConsoleProtocol() 
        {
            var server = CreateSocketServer<TextPackageInfo, LinePipelineFilter>(packageHandler: (s, p) =>
            {
                s.Channel.SendAsync(Encoding.UTF8.GetBytes("Hello World\r\n"));
            });
            
            Assert.True(await server.StartAsync());
            Assert.Equal(0, server.SessionCount);

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
            
            using (var stream = new NetworkStream(client))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true))
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024 * 1024 * 4))
            {
                await streamWriter.WriteAsync("Hello World\r\n");
                await streamWriter.FlushAsync();
                var line = await streamReader.ReadLineAsync();
                Assert.Equal("Hello World", line);
            }

            await server.StopAsync();
        }
    }
}
