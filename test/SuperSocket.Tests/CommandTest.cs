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
using SuperSocket.Tests.Command;

namespace SuperSocket.Tests
{
    [Trait("Category", "Command")]
    public class CommandTest : TestClassBase
    {
        public CommandTest(ITestOutputHelper outputHelper)
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
                .UseCommand(commandOptions =>
                {
                    // register commands one by one
                    commandOptions.AddCommand<ADD>();
                    commandOptions.AddCommand<MULT>();
                    commandOptions.AddCommand<SUB>();

                    // register all commands in one assembly
                    //commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
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
        public async Task TestCommandsFromAssembly(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
                .UseCommand(commandOptions =>
                {
                    // register all commands in one assembly
                    commandOptions.AddCommandAssembly(typeof(MIN).GetTypeInfo().Assembly);
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
                    await streamWriter.WriteAsync("MIN 8 6 3\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("3", line);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommandsFromConfigAssembly(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
                .UseCommand(commandOptions =>
                {
                    // register all commands in one assembly
                    commandOptions.Assemblies = new [] { new CommandAssemblyConfig { Name = "SuperSocket.Tests.Command" } };
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
                    await streamWriter.WriteAsync("MIN 8 6 3\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("3", line);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [Trait("Category", "JsonCommands")]
        public async Task TestJsonCommands(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
                .UseCommand(commandOptions =>
                {
                    // register commands one by one
                    commandOptions.AddCommand<POW>();
                    commandOptions.AddCommand<MAX>();

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
                    await streamWriter.WriteAsync("POW { \"x\": 2, \"y\": 2 }\r\n");
                    await streamWriter.FlushAsync();
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("4", line);

                    await streamWriter.WriteAsync("MAX { \"numbers\": [ 45, 77, 6, 88, 46 ] }\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("88", line);
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
                .UseCommand(commandOptions =>
                {
                    // register commands one by one
                    /*
                    commandOptions.AddCommand<ADD>();
                    commandOptions.AddCommand<MULT>();
                    commandOptions.AddCommand<SUB>();
                    commandOptions.AddCommand<DIV>();
                    */
                    // register all commands in one assembly
                    commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
                })
                .UseSession<MySession>()
                .BuildAsServer())
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

        class HitCountCommandFilterAttribute : AsyncCommandFilterAttribute
        {
            private static int _total;

            public static int Total
            {
                get { return _total; }
            }

            public override ValueTask OnCommandExecutedAsync(CommandExecutingContext commandContext)
            {
                Interlocked.Increment(ref _total);
                return new ValueTask();
            }

            public override ValueTask<bool> OnCommandExecutingAsync(CommandExecutingContext commandContext)
            {
                return new ValueTask<bool>(true);
            }
        }

        class IncreaseCountCommandFilterAttribute : AsyncCommandFilterAttribute
        {
            public override ValueTask OnCommandExecutedAsync(CommandExecutingContext commandContext)
            {
                return new ValueTask();
            }

            public override ValueTask<bool> OnCommandExecutingAsync(CommandExecutingContext commandContext)
            {
                var sessionState = commandContext.Session.DataContext as SessionState;
                sessionState.ExecutionCount++;
                return new ValueTask<bool>(true);
            }
        }

        class DecreaseCountCommandFilterAttribute : CommandFilterAttribute
        {
            public override void OnCommandExecuted(CommandExecutingContext commandContext)
            {
      
            }

            public override bool OnCommandExecuting(CommandExecutingContext commandContext)
            {
                var sessionState = commandContext.Session.DataContext as SessionState;
                sessionState.ExecutionCount--;
                return true;
            }
        }

        [IncreaseCountCommandFilter]
        class COUNT : IAsyncCommand<StringPackageInfo>
        {
            private IPackageEncoder<string> _encoder;

            public COUNT(IPackageEncoder<string> encoder)
            {
                _encoder = encoder;
            }

            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                await session.SendAsync(_encoder, "OK\r\n");
            }
        }

        [DecreaseCountCommandFilter]
        class COUNTDOWN : IAsyncCommand<StringPackageInfo>
        {
            private IPackageEncoder<string> _encoder;

            public COUNTDOWN(IPackageEncoder<string> encoder)
            {
                _encoder = encoder;
            }

            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                await session.SendAsync(_encoder, "OK\r\n");
            }
        }

        class SessionState
        {
            public int ExecutionCount { get; set; }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommandFilter(Type hostConfiguratorType)
        {
            var sessionState = new SessionState();

            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>(hostConfigurator)
                .UseCommand(commandOptions =>
                {
                    commandOptions.AddCommand<COUNT>();
                    commandOptions.AddCommand<COUNTDOWN>();
                    commandOptions.AddGlobalCommandFilter<HitCountCommandFilterAttribute>();
                })
                .UseSessionHandler((s) =>
                {
                    s.DataContext = sessionState;
                    return new ValueTask();
                })
                .BuildAsServer())
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
                    var currentTotal = HitCountCommandFilterAttribute.Total;

                    for (var i = 1; i <= 100; i++)
                    {
                        await streamWriter.WriteAsync("COUNT\r\n");
                        await streamWriter.FlushAsync();
                        var line = await streamReader.ReadLineAsync();

                        Assert.Equal(i, sessionState.ExecutionCount);
                    }

                    Assert.Equal(currentTotal + 100, HitCountCommandFilterAttribute.Total);

                    for (var i = 99; i >= 0; i--)
                    {
                        await streamWriter.WriteAsync("COUNTDOWN\r\n");
                        await streamWriter.FlushAsync();
                        var line = await streamReader.ReadLineAsync();

                        Assert.Equal(i, sessionState.ExecutionCount);
                    }

                    Assert.Equal(currentTotal + 200, HitCountCommandFilterAttribute.Total);
                }

                await server.StopAsync();
            }
        }
    }
}
