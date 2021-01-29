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
using SuperSocket.Channel;

namespace SuperSocket.Tests
{
    [Trait("Category", "SessionContainer")]
    public class SessionContainerTest : TestClassBase
    {
        class SESS : IAsyncCommand<StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                await session.SendAsync(Encoding.UTF8.GetBytes(session.SessionID + "\r\n"));
            }
        }

        class MyPipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
        {
            public MyPipelineFilter()
                : base(new[] { (byte)'\r', (byte)'\n' })
            {

            }

            protected override StringPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
            {
                var text = buffer.GetString(Encoding.UTF8);
                var parts = text.Split(' ');

                return new StringPackageInfo
                {
                    Key = parts[0],
                    Parameters = parts.Skip(1).ToArray()
                };
            }
        }

        public SessionContainerTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        public class SessionContainerDependentService
        {
            public IAsyncSessionContainer AsyncSessionContainer { get; private set; }
            public ISessionContainer SessionContainer { get; private set; }            
            public SessionContainerDependentService(ISessionContainer sessionContainer, IAsyncSessionContainer asyncSessionContainer)
            {
                SessionContainer = sessionContainer;
                AsyncSessionContainer = asyncSessionContainer;
            }
        }

        [Fact] 
        public async Task TestInProcSessionContainer()
        {
            var hostConfigurator = new RegularHostConfigurator();
            using (var server = CreateSocketServerBuilder<StringPackageInfo, MyPipelineFilter>(hostConfigurator)
                .UseCommand<string, StringPackageInfo>(commandOptions =>
                {
                    commandOptions.AddCommand<SESS>();
                })
                .UseInProcSessionContainer()
                .ConfigureServices((ctx, services) =>
                    services.AddSingleton<SessionContainerDependentService>()                
                )
                .BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                var sessionContainer = server.GetSessionContainer();
                Assert.NotNull(sessionContainer);

                var sessionContainerDependentService = server.ServiceProvider.GetService<SessionContainerDependentService>();
                Assert.NotNull(sessionContainerDependentService);
                Assert.NotNull(sessionContainerDependentService.SessionContainer);
                var asyncSessionContainer = sessionContainerDependentService.AsyncSessionContainer as SyncToAsyncSessionContainerWrapper;
                Assert.NotNull(asyncSessionContainer);
                Assert.Same(sessionContainerDependentService.SessionContainer, asyncSessionContainer.SessionContainer);
                Assert.Same(sessionContainer, sessionContainerDependentService.SessionContainer);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(hostConfigurator.GetServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                Thread.Sleep(1000);

                Assert.Equal(1, sessionContainer.GetSessionCount());

                var sessionID = string.Empty;

                var closed = false;

                using (var stream = await hostConfigurator.GetClientStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("SESS\r\n");
                    await streamWriter.FlushAsync();
                    sessionID = await streamReader.ReadLineAsync();

                    Assert.False(string.IsNullOrEmpty(sessionID));
                    
                    var session = sessionContainer.GetSessionByID(sessionID);

                    Assert.NotNull(session);
                    Assert.Equal(sessionID, session.SessionID);

                    session.Closed +=  (s, e) =>
                    {
                        closed = true;
                        return new ValueTask();
                    };

                    await session.Channel.CloseAsync(CloseReason.LocalClosing);
                }

                await Task.Delay(1000);
                Assert.Equal(0, sessionContainer.GetSessionCount());
                Assert.Null(sessionContainer.GetSessionByID(sessionID));
                Assert.True(closed);
                
                await server.StopAsync();
            }
        }
    }
}
