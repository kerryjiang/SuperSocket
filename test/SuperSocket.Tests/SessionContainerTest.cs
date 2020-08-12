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

        [Fact] 
        public async Task TestInProcSessionContainer()
        {
            using (var server = CreateSocketServerBuilder<StringPackageInfo, MyPipelineFilter>()
                .UseCommand<string, StringPackageInfo>(commandOptions =>
                {
                    commandOptions.AddCommand<SESS>();
                })
                .UseInProcSessionContainer()
                .BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                var sessionContainer = server.ServiceProvider.GetSessionContainer();
                Assert.NotNull(sessionContainer);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));
                OutputHelper.WriteLine("Connected.");

                Thread.Sleep(1000);

                Assert.Equal(1, sessionContainer.GetSessionCount());

                var sessionID = string.Empty;

                var closed = false;

                using (var stream = new NetworkStream(client))
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

                    await session.Channel.CloseAsync();
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
