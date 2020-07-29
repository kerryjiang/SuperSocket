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
using SuperSocket.Server;
using System.Threading;
using SuperSocket.Test.Command;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    [Trait("Category", "Autofac")]
    public class AutofacTest : TestClassBase
    {
        public class MySession : AppSession
        {

        }
        
        public class ADD : IAsyncCommand<StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Sum();

                await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        public class MULT : IAsyncCommand<StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x * y);

                await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        public class SUB : IAsyncCommand<StringPackageInfo>
        {
            private IPackageEncoder<string> _encoder;

            public SUB(IPackageEncoder<string> encoder)
            {
                _encoder = encoder;
            }

            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x - y);
                
                // encode the text message by encoder
                await session.SendAsync(_encoder, result.ToString() + "\r\n");
            }
        }

        public class DIV : IAsyncCommand<MySession, StringPackageInfo>
        {
            private IPackageEncoder<string> _encoder;

            public DIV(IPackageEncoder<string> encoder)
            {
                _encoder = encoder;
            }

            public async ValueTask ExecuteAsync(MySession session, StringPackageInfo package)
            {
                var values = package
                    .Parameters
                    .Select(p => int.Parse(p))
                    .ToArray();

                var result = values[0] / values[1];

                var socketSession = session as IAppSession;
                // encode the text message by encoder
                await socketSession.SendAsync(_encoder, result.ToString() + "\r\n");
            }
        }

        public class PowData
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class POW : JsonAsyncCommand<IAppSession, PowData>
        {
            protected override async ValueTask ExecuteJsonAsync(IAppSession session, PowData data)
            {
                await session.SendAsync(Encoding.UTF8.GetBytes($"{Math.Pow(data.X, data.Y)}\r\n"));
            }
        }

        public class MaxData
        {
            public int[] Numbers { get; set; }
        }

        public class MAX : JsonAsyncCommand<IAppSession, MaxData>
        {
            protected override async ValueTask ExecuteJsonAsync(IAppSession session, MaxData data)
            {
                var maxValue = data.Numbers.OrderByDescending(i => i).FirstOrDefault();
                await session.SendAsync(Encoding.UTF8.GetBytes($"{maxValue}\r\n"));
            }
        }

        public AutofacTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommands(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
                .UseCommand()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterType<ADD>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                    builder.RegisterType<MULT>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                    builder.RegisterType<SUB>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                })
                .BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");


                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");

                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("ADD 1 2 3\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("6", line);

                    await streamWriter.WriteAsync("MULT 2 5\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("10", line);

                    await streamWriter.WriteAsync("SUB 8 2\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("6", line);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommandsWithCustomSession(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
                .UseCommand()
                .UseSession<MySession>()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterType<ADD>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                    builder.RegisterType<MULT>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                    builder.RegisterType<SUB>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                    builder.RegisterType<DIV>().As<IAsyncCommand<MySession, StringPackageInfo>>();
                })                
                .BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");


                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");

                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("ADD 1 2 3\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("6", line);

                    await streamWriter.WriteAsync("MULT 2 5\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("10", line);

                    await streamWriter.WriteAsync("SUB 8 2\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("6", line);

                    await streamWriter.WriteAsync("DIV 8 2\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("4", line);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [Trait("Category", "Autofac.MultipleServerHost")]
        public async Task TestCommandsWithCustomSessionMultipleServerHost(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = MultipleServerHostBuilder.Create()
                .ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.Sources.Clear();
                    configApp.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:name", "TestServer" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:port", "4040" }
                    });
                })
                .AddServer<StringPackageInfo, CommandLinePipelineFilter>(builder =>
                {
                    builder
                    .UseCommand()
                    .UseSession<MySession>()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer<ContainerBuilder>(builder =>
                    {
                        builder.RegisterType<ADD>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                        builder.RegisterType<MULT>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                        builder.RegisterType<SUB>().As<IAsyncCommand<IAppSession, StringPackageInfo>>();
                        builder.RegisterType<DIV>().As<IAsyncCommand<MySession, StringPackageInfo>>();
                    });
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");

                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("ADD 1 2 3\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("6", line);

                    await streamWriter.WriteAsync("MULT 2 5\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("10", line);

                    await streamWriter.WriteAsync("SUB 8 2\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("6", line);

                    await streamWriter.WriteAsync("DIV 8 2\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("4", line);
                }

                await server.StopAsync();
            }
        }
    }
}
