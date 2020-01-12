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

namespace Tests
{
    [Trait("Category", "Command")]
    public class CommandTest : TestClassBase
    {
        class ADD : IAsyncCommand<StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Sum();

                await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        class MULT : IAsyncCommand<StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x * y);

                await session.SendAsync(Encoding.UTF8.GetBytes(result.ToString() + "\r\n"));
            }
        }

        class SUB : IAsyncCommand<StringPackageInfo>
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

                    // register all commands in one aassembly
                    //commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
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
                }

                await server.StopAsync();
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
                return new ValueTask<bool>(false);
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
                return false;
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
                })
                .ConfigureSessionHandler((s) =>
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
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");

                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    for (var i = 1; i <= 100; i++)
                    {
                        await streamWriter.WriteAsync("COUNT\r\n");
                        await streamWriter.FlushAsync();
                        var line = await streamReader.ReadLineAsync();

                        Assert.Equal(i, sessionState.ExecutionCount);
                    }

                    for (var i = 99; i >= 0; i--)
                    {
                        await streamWriter.WriteAsync("COUNTDOWN\r\n");
                        await streamWriter.FlushAsync();
                        var line = await streamReader.ReadLineAsync();

                        Assert.Equal(i, sessionState.ExecutionCount);
                    }
                }

                await server.StopAsync();
            }
        }
    }
}
