using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using System.Threading;

namespace SuperSocket.Tests
{
    [Trait("Category", "ServerOptions")]
    public class ServerOptionsTest : TestClassBase
    {
        public ServerOptionsTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task MaxPackageLength(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .ConfigureSuperSocket((options) =>
                {
                    options.MaxPackageLength = 100;
                })
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");


                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(hostConfigurator.GetServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        await streamWriter.WriteAsync(Guid.NewGuid().ToString());
                    }

                    await streamWriter.WriteAsync("\r\n");
                    await streamWriter.FlushAsync();

                    Thread.Sleep(1000);

                    var line = await streamReader.ReadLineAsync();

                    Assert.Null(line);
                }

                await server.StopAsync();
            }
        }
    }
}
