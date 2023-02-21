using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Buffers;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.WebSocket.Server;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket;
using SuperSocket.Server;
using SuperSocket.Tests.Command;
using Xunit;
using Xunit.Abstractions;


namespace SuperSocket.Tests.WebSocket
{
    [Trait("Category", "WebSocketBasic")]
    public class WebSocketBasicTest : WebSocketServerTestBase
    {
        private Encoding _encoding = new UTF8Encoding(false);

        public WebSocketBasicTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestCustomWebSocketSession(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var session = default(WebSocketSession);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder
                    .UseSession<MyWebSocketSession>()
                    .UseSessionHandler(async (s) =>
                    {
                        session = s as WebSocketSession;
                        await Task.CompletedTask;
                    })
                    .ConfigureServices(
                        (hostCtx, services) =>
                        {
                            services.AddSingleton<ITestService, TestService>();
                        }
                    );
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1 * 1000);

                Assert.IsType<MyWebSocketSession>(session);
                Assert.IsType<TestService>((session as MyWebSocketSession).TestService);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [Trait("Category", "WebSocketHandshake")]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestHandshake(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var serverSessionPath = string.Empty;
            var connected = false;

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                builder.UseSessionHandler((s) =>
                {
                    serverSessionPath = (s as WebSocketSession).Path;
                    connected = true;
                    return new ValueTask();
                },
                (s, e) =>
                {
                    connected = false;
                    return new ValueTask();
                });

                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var path = "/app/talk";

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}" + path), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1 * 1000);
                // test path
                Assert.Equal(path, serverSessionPath);
                Assert.True(connected);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);
                Assert.False(connected);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [Trait("Category", "WebSocketHandshake")]
        public async Task TestFalseHandshake(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(
                hostConfigurator: hostConfigurator)
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure<HandshakeOptions>(options =>
                    {
                        options.CheckingInterval = 1;
                        options.OpenHandshakeTimeOut = 60;
                        options.HandshakeValidator = (session, package) => new ValueTask<bool>(false);
                    });
                })
                .UseInProcSessionContainer()
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var sessionContainer = server.GetSessionContainer();

                Assert.NotNull(sessionContainer);

                var websocketMiddleware = server.ServiceProvider.GetRequiredService<IWebSocketServerMiddleware>();
                
                await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (index, ct) =>
                {
                    var websocket = new ClientWebSocket();
                    websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    try
                    {
                        await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}/test"), CancellationToken.None);
                    }
                    catch (WebSocketException)
                    {
                    }

                    Assert.Equal(WebSocketState.Closed, websocket.State);
                });
                
                Assert.Equal(0, server.SessionCount);
                Assert.Equal(0, sessionContainer.GetSessionCount());

                await Task.Delay(1000);

                Assert.Equal(0, websocketMiddleware.OpenHandshakePendingQueueLength);
                Assert.Equal(0, websocketMiddleware.CloseHandshakePendingQueueLength);

                await server.StopAsync();
            }
        }

        [Theory]
        [Trait("Category", "TestEmptyMessage")]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestEmptyMessage(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.UseWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(message.Message);
                });
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var data = new byte[0];
                var segment = new ArraySegment<byte>(data, 0, data.Length);

                await websocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                
                var receiveBuffer = new byte[1024 * 1024 * 4];

                var receivedMessage = await GetWebSocketReply(websocket, receiveBuffer);             

                Assert.Equal(0, receivedMessage.Length);

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }


        [Theory]
        [Trait("Category", "TestLongMessageFromServer")]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestLongMessageFromServer(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var serverSessionPath = string.Empty;

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                builder.UseWebSocketMessageHandler(async (s,  p) =>
                {
                    await (s as WebSocketSession).SendAsync(p.Message);
                }).UseSessionHandler((s) =>
                {
                    serverSessionPath = (s as WebSocketSession).Path;
                    return new ValueTask();
                });

                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var path = "/app/talk";

                var websocket = new ClientWebSocket();

                var texts = new string[]
                {
                    "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                    "这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本",
                    "这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本这是一段长文本",
                    "这是一段长aaaaaaaaaaa文本这是一段长文本这是一段长文aaaaaaaaaaa本这是一段长文本这是一段长文本这aaaaaaaaaaa是一段长文本这是aaaaaaaaaaa一段长文本aaaaaaaaaaa这是一段长文本这是一段长aaaaaaaaaaa文本这是一段长文本这是一段长文本这是一段长文本"
                };

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}" + path), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var buffer = new byte[1024 * 1024 * 4];
                var segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
                
                
                foreach (var t in texts)
                {
                    var len = Utf8Encoding.GetBytes(t, 0, t.Length, buffer, 0);
                    await websocket.SendAsync(new ArraySegment<byte>(buffer, 0, len), WebSocketMessageType.Text, true, CancellationToken.None);
                    var reply = await this.GetWebSocketReply(websocket, buffer);
                    Assert.Equal(t, reply);
                }

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        
        [Trait("Category", "WebSocketHandshakeTimeOut")]
        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestHandshakeTimeOut(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                builder.ConfigureServices((ctx, services) =>
                {
                    services.Configure<HandshakeOptions>(options =>
                    {
                        options.CheckingInterval = 1;
                        options.OpenHandshakeTimeOut = 1;
                    });
                });
                return builder;
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;
                var endPoint = hostConfigurator.GetServerEndPoint();
                await socket.ConnectAsync(endPoint);
                Assert.True(socket.Connected);
                Assert.False(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

                /* enable tcp keep alive
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1);
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 1);
                */

                await Task.Delay(1000 * 5);
                //Assert.False(socket.Connected);
                Assert.True(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);

                await server.StopAsync();
            }
        }


        [Theory]
        [InlineData(typeof(RegularHostConfigurator), 10)]
        [InlineData(typeof(SecureHostConfigurator), 10)]
        public async Task TestTextMessageSendReceive(Type hostConfiguratorType, int connCount) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.UseWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(message.Message);
                });
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var tasks = Enumerable.Range(0, connCount).Select((x, y) => 
                {
                    return Task.Run(async () =>
                    {

                        var websocket = new ClientWebSocket();

                        websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                        var startConnectTime = DateTime.Now;
                        await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);
                        OutputHelper.WriteLine($"Took {DateTime.Now.Subtract(startConnectTime)} to establish the connection from client side.");

                        Assert.Equal(WebSocketState.Open, websocket.State);

                        var receiveBuffer = new byte[256];

                        for (var i = 0; i < 100; i++)
                        {
                            var message = Guid.NewGuid().ToString();
                            var data = _encoding.GetBytes(message);
                            var segment = new ArraySegment<byte>(data, 0, data.Length);

                            await websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                            
                            var receivedMessage = await GetWebSocketReply(websocket, receiveBuffer);

                            OutputHelper.WriteLine(receivedMessage);
                            Assert.Equal(message, receivedMessage);
                        }

                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                        Assert.Equal(WebSocketState.Closed, websocket.State);
                    });
                });

                await Task.WhenAll(tasks.ToArray());
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator), 10)]
        [InlineData(typeof(SecureHostConfigurator), 10)]
        public async Task TestBinaryMessageSendReceive(Type hostConfiguratorType, int connCount) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.UseWebSocketMessageHandler(async (session, message) =>
                {
                    message.OpCode = OpCode.Binary;
                    await session.SendAsync(message);
                });
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var tasks = Enumerable.Range(0, connCount).Select((x, y) => 
                {
                    return Task.Run(async () =>
                    {
                        var websocket = new ClientWebSocket();

                        websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                        var startConnectTime = DateTime.Now;
                        await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);
                        OutputHelper.WriteLine($"Took {DateTime.Now.Subtract(startConnectTime)} to establish the connection from client side.");

                        if (websocket.State != WebSocketState.Open)
                        {
                            return new TaskTestResult
                            {
                                Result = false,
                                Message = "WebSocket is not opened."
                            };
                        }

                        var receiveBuffer = new byte[256];

                        for (var i = 0; i < 100; i++)
                        {
                            var message = Guid.NewGuid().ToString();
                            var data = _encoding.GetBytes(message);
                            var segment = new ArraySegment<byte>(data, 0, data.Length);

                            await websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Binary, true, CancellationToken.None);                    
                            var receivedMessage = await GetWebSocketReply(websocket, receiveBuffer, WebSocketMessageType.Binary);

                            if (!receivedMessage.Equals(message))
                            {
                                return new TaskTestResult
                                {
                                    Result = false,
                                    Message = "The received message is not correct."
                                };
                            }
                        }

                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                        if (websocket.State != WebSocketState.Closed)
                        {
                            return new TaskTestResult
                            {
                                Result = false,
                                Message = "WebSocket is not closed."
                            };
                        }

                        return new TaskTestResult { Result = true };
                    });
                });

                var taskResults = await Task.WhenAll(tasks.ToArray());

                foreach (var r in taskResults)
                {
                    Assert.True(r.Result, r.Message);
                }
                
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator), 10)]
        [InlineData(typeof(SecureHostConfigurator), 10)]
        public async Task TestBinaryMessageToArray(Type hostConfiguratorType, int connCount) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.UseWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(_encoding.GetString(message.Data.ToArray()));
                });
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var tasks = Enumerable.Range(0, connCount).Select((x, y) => 
                {
                    return Task.Run(async () =>
                    {
                        var websocket = new ClientWebSocket();

                        websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                        var startConnectTime = DateTime.Now;
                        await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);
                        OutputHelper.WriteLine($"Took {DateTime.Now.Subtract(startConnectTime)} to establish the connection from client side.");

                        if (websocket.State != WebSocketState.Open)
                        {
                            return new TaskTestResult
                            {
                                Result = false,
                                Message = "WebSocket is not opened."
                            };
                        }

                        var receiveBuffer = new byte[256];

                        for (var i = 0; i < 100; i++)
                        {
                            var message = Guid.NewGuid().ToString();
                            var data = _encoding.GetBytes(message);
                            var segment = new ArraySegment<byte>(data, 0, data.Length);

                            await websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Binary, true, CancellationToken.None);                    
                            var receivedMessage = await GetWebSocketReply(websocket, receiveBuffer, WebSocketMessageType.Text);

                            if (!receivedMessage.Equals(message))
                            {
                                return new TaskTestResult
                                {
                                    Result = false,
                                    Message = "The received message is not correct."
                                };
                            }
                        }

                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                        if (websocket.State != WebSocketState.Closed)
                        {
                            return new TaskTestResult
                            {
                                Result = false,
                                Message = "WebSocket is not closed."
                            };
                        }

                        return new TaskTestResult { Result = true };
                    });
                });                

                var taskResults = await Task.WhenAll(tasks.ToArray());

                foreach (var r in taskResults)
                {
                    Assert.True(r.Result, r.Message);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [Trait("Category", "TestVariousSizeMessagesConcurrent")]
        [InlineData(typeof(RegularHostConfigurator), 10)]
        [InlineData(typeof(SecureHostConfigurator), 10)]
        public async Task TestVariousSizeMessagesConcurrent(Type hostConfiguratorType, int connCount) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.UseWebSocketMessageHandler(async (session, message) =>
                {
                    await session.SendAsync(message.Message);
                }).UseSessionHandler(async (s) =>
                {
                    await (s as WebSocketSession).SendAsync(s.SessionID);
                }).ConfigureSuperSocket((options) =>
                {
                    foreach (var l in options.Listeners)
                    {
                        l.BackLog = connCount;
                    }
                }) as WebSocketHostBuilder;
            }, hostConfigurator).BuildAsServer())
            {
                var loggerFactory = server.ServiceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Test");

                Assert.True(await server.StartAsync());
                logger.LogInformation("Server started.");

                var tasks = Enumerable.Range(0, connCount).Select((x, y) => 
                {
                    return Task.Run(async () =>
                    {
                        var websocket = new ClientWebSocket();

                        websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                        var startConnectTime = DateTime.Now;
                        await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);
                        OutputHelper.WriteLine($"Took {DateTime.Now.Subtract(startConnectTime)} to establish the connection from client side.");

                        Assert.Equal(WebSocketState.Open, websocket.State);

                        var msgSize = 1024 * 4;
                        var receiveBuffer = new byte[msgSize * 2];
                        var sessionID = await GetWebSocketReply(websocket, receiveBuffer);

                        var msgBuilder = new StringBuilder();                        

                        while (msgBuilder.Length < msgSize)
                        {
                            msgBuilder.Append(Guid.NewGuid().ToString().Replace("-", string.Empty));
                        }

                        var msg = msgBuilder.ToString(0, msgSize).ToCharArray();

                        var sendBuffer = new byte[msgSize * 2];

                        for (var i = 1; i <= msgSize; i++)
                        {
                            var len = _encoding.GetBytes(msg, 0, i, sendBuffer, 0);
                            var segment = new ArraySegment<byte>(sendBuffer, 0, len);

                            await websocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                            var reply = await GetWebSocketReply(websocket, receiveBuffer);

                            Assert.Equal(new string(msg, 0, i), reply);
                        }

                        await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        Assert.Equal(WebSocketState.Closed, websocket.State);
                    });
                });

                await Task.WhenAll(tasks.ToArray());
                await server.StopAsync();
            }
        }

        [Theory]
        [Trait("Category", "TestVariousSizeMessages")]
        [InlineData(typeof(RegularHostConfigurator), 125, 2)]
        [InlineData(typeof(RegularHostConfigurator), 126, 2)]
        [InlineData(typeof(RegularHostConfigurator), 127, 2)]
        [InlineData(typeof(RegularHostConfigurator), 65535, 2)]
        [InlineData(typeof(RegularHostConfigurator), 65536, 2)]
        [InlineData(typeof(RegularHostConfigurator), 65537, 2)]
        [InlineData(typeof(RegularHostConfigurator), 8192, 1, 1024)]
        public async Task TestVariousSizeMessages(Type hostConfiguratorType, int messageLength, int repeat, int fragmentSize = 100000)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);            

            var sb = new StringBuilder();
            var rd = new Random();

            for (var i = 0; i < messageLength; i++)
            {
                sb.Append((char)('a' + rd.Next(0, 26)));
            }

            var textToSent = sb.ToString();

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder.UseWebSocketMessageHandler(async (session, message) =>
                {                
                    await session.SendAsync(message.Message);
                }).ConfigureSuperSocket(s =>
                {
                    s.MaxPackageLength = 0;
                    s.ReceiveBufferSize = 2048;
                });
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                await server.StartAsync();
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var receiveBuffer = new byte[1024 * 1024 * 4];

                for (var i = 0; i < repeat; i++)
                {
                    var data = Encoding.UTF8.GetBytes(textToSent);
                    var rest = data.Length;
                    var offset = 0;

                    while (rest > 0)
                    {
                        var length = Math.Min(rest, fragmentSize);
                        var segment = new ArraySegment<byte>(data, offset, length);
                        offset += length;
                        rest -= length;

                        var endOfMessage = rest == 0;

                        await websocket.SendAsync(segment, WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
                        await Task.Delay(500);
                    }
                    
                    var receivedMessage = await GetWebSocketReply(websocket, receiveBuffer);
                    Assert.Equal(textToSent, receivedMessage);
                }

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);                
                await Task.Delay(TimeSpan.FromSeconds(1));
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommands(Type hostConfiguratorType, Func<ISuperSocketHostBuilder<WebSocketPackage>, ISuperSocketHostBuilder<WebSocketPackage>> hostBuilderAdapter = null) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                var hostBuilder = builder
                    .UseCommand<StringPackageInfo, StringPackageConverter>(commandOptions =>
                    {
                        // register commands one by one
                        commandOptions.AddCommand<ADD>();
                        commandOptions.AddCommand<MULT>();
                        commandOptions.AddCommand<SUB>();

                        // register all commands in one assembly
                        //commandOptions.AddCommandAssembly(typeof(SUB).GetTypeInfo().Assembly);
                    });

                if (hostBuilderAdapter != null)
                    hostBuilder = hostBuilderAdapter(hostBuilder);

                return hostBuilder;
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                var receiveBuffer = new byte[256];

                Assert.Equal("11", await GetWebSocketReply(websocket, receiveBuffer, "ADD 5 6"));
                Assert.Equal("8", await GetWebSocketReply(websocket, receiveBuffer, "SUB 10 2"));
                Assert.Equal("21", await GetWebSocketReply(websocket, receiveBuffer, "MULT 3 7"));

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCommandsWithCustomSession(Type hostConfiguratorType) 
        {
            await TestCommands(hostConfiguratorType, builder => 
            {
                return builder
                    .UseSession<MyWebSocketSession>()
                    .ConfigureServices(
                        (hostCtx, services) =>
                        {
                            services.AddSingleton<ITestService, TestService>();
                        }
                    );
            });
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestProtocols(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketServerBuilder(builder =>
            {
                return builder
                    .UseCommand<StringPackageInfo, StringPackageConverter>("test1", commandOptions =>
                    {
                        // register commands one by one
                        commandOptions.AddCommand<ADD>();
                        commandOptions.AddCommand<MULT>();
                        commandOptions.AddCommand<SUB>();
                    })
                    .UseWebSocketMessageHandler("test2", async (s, p) =>
                    {
                        // echo back the message
                        await s.SendAsync(p.Message);
                    });
            }, hostConfigurator).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                websocket.Options.AddSubProtocol("test1");
                websocket.Options.AddSubProtocol("test2");

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                Assert.Equal("test1", websocket.SubProtocol);

                var receiveBuffer = new byte[256];

                Assert.Equal("11", await GetWebSocketReply(websocket, receiveBuffer, "ADD 5 6"));
                Assert.Equal("8", await GetWebSocketReply(websocket, receiveBuffer, "SUB 10 2"));
                Assert.Equal("21", await GetWebSocketReply(websocket, receiveBuffer, "MULT 3 7"));

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                websocket = new ClientWebSocket();

                websocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                websocket.Options.AddSubProtocol("test2");
                websocket.Options.AddSubProtocol("test1");

                await websocket.ConnectAsync(new Uri($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                Assert.Equal("test2", websocket.SubProtocol);

                Assert.Equal("ADD 5 6", await GetWebSocketReply(websocket, receiveBuffer, "ADD 5 6"));
                Assert.Equal("SUB 10 2", await GetWebSocketReply(websocket, receiveBuffer, "SUB 10 2"));
                Assert.Equal("MULT 3 7", await GetWebSocketReply(websocket, receiveBuffer, "MULT 3 7"));
                
                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        class MySocketService : SuperSocketService<StringPackageInfo>
        {
            public MySocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
            {
            }
        }

        class MyWebSocketService : SuperSocketService<WebSocketPackage>
        {
            public MyWebSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
            {
            }
        }

        [Fact]
        [Trait("Category", "TestWebSocketMultipleServerHost")]
        public async Task TestMultipleServerHost()
        {
            var serverName1 = "TestServer1";
            var serverName2 = "TestServer2";

            var hostBuilder = MultipleServerHostBuilder.Create()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddJsonFile("Config/multiple_server.json", optional: false, reloadOnChange: true);
                })
                .AddServer<MySocketService, StringPackageInfo, CommandLinePipelineFilter>(builder =>
                {
                    builder
                    .ConfigureServerOptions((ctx, config) =>
                    {
                        return config.GetSection(serverName1);
                    }).UseSessionHandler(async (s) =>
                    {
                        await s.SendAsync(Utf8Encoding.GetBytes($"{s.Server.Name}\r\n"));
                    })
                    .UseCommand(commandOptions =>
                    {
                        // register all commands in one assembly
                        commandOptions.AddCommandAssembly(typeof(MIN).GetTypeInfo().Assembly);
                    });
                })
                .AddWebSocketServer<MyWebSocketService>(builder =>
                {
                    builder
                    .ConfigureServerOptions((ctx, config) =>
                    {
                        return config.GetSection(serverName2);
                    })
                    .UseCommand<StringPackageInfo, StringPackageConverter>(commandOptions =>
                    {
                        commandOptions.AddCommand<ADD>();
                        commandOptions.AddCommand<MULT>();
                        commandOptions.AddCommand<SUB>();
                    });
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                });

            using(var host = hostBuilder.Build())
            {
                await host.StartAsync();

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, DefaultServerPort));
                
                using (var stream = new NetworkStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal(serverName1, line);

                    await streamWriter.WriteAsync("MIN 8 6 3\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("3", line);

                    await streamWriter.WriteAsync("SORT 8 6 3\r\n");
                    await streamWriter.FlushAsync();
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("SORT 3 6 8", line);
                }
                
                var websocket = new ClientWebSocket();

                await websocket.ConnectAsync(new Uri($"ws://localhost:{AlternativeServerPort}"), CancellationToken.None);
                Assert.Equal(WebSocketState.Open, websocket.State);
                
                var receiveBuffer = new byte[256];

                Assert.Equal("11", await GetWebSocketReply(websocket, receiveBuffer, "ADD 5 6"));
                Assert.Equal("8", await GetWebSocketReply(websocket, receiveBuffer, "SUB 10 2"));
                Assert.Equal("21", await GetWebSocketReply(websocket, receiveBuffer, "MULT 3 7"));

                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await host.StopAsync();
            }
        }


        [Fact]
        [Trait("Category", "TestWebSocketStartByHost")]
        public async Task TestStartByHost()
        {
            var hostBuilder = WebSocketHostBuilder.Create()
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                });

            using(var host = hostBuilder.Build())
            {
                await host.StartAsync();
               
                var websocket = new ClientWebSocket();

                await websocket.ConnectAsync(new Uri($"ws://localhost:{DefaultServerPort}"), CancellationToken.None);

                Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1000 * 5);
                
                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);                
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await host.StopAsync();
            }
        }

        private async ValueTask<string> GetWebSocketReply(ClientWebSocket websocket, byte[] receiveBuffer)
        {
            return await GetWebSocketReply(websocket, receiveBuffer, WebSocketMessageType.Text);
        }

        private async ValueTask<string> GetWebSocketReply(ClientWebSocket websocket, byte[] receiveBuffer, WebSocketMessageType messageType)
        {
            var sb = new StringBuilder();

            while (true)
            {
                var receiveSegment = new ArraySegment<byte>(receiveBuffer, 0, receiveBuffer.Length);
                var result = await websocket.ReceiveAsync(receiveSegment, CancellationToken.None);

                Assert.Equal(messageType, result.MessageType);

                sb.Append(_encoding.GetString(receiveBuffer, 0, result.Count));

                if (result.EndOfMessage)
                    break;
            }

            return sb.ToString();
        }

        private async ValueTask<string> GetWebSocketReply(ClientWebSocket websocket, byte[] receiveBuffer, string request)
        {
            var data = _encoding.GetBytes(request);
            var segment = new ArraySegment<byte>(data, 0, data.Length);

            await websocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            
            return await GetWebSocketReply(websocket, receiveBuffer);
        }

        class ADD : IAsyncCommand<WebSocketSession, StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(WebSocketSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Sum();

                await session.SendAsync(result.ToString());
            }
        }

        class MULT : IAsyncCommand<WebSocketSession, StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(WebSocketSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x * y);

                await session.SendAsync(result.ToString());
            }
        }

        class SUB : IAsyncCommand<WebSocketSession, StringPackageInfo>
        {
            public async ValueTask ExecuteAsync(WebSocketSession session, StringPackageInfo package)
            {
                var result = package.Parameters
                    .Select(p => int.Parse(p))
                    .Aggregate((x, y) => x - y);

                await session.SendAsync(result.ToString());
            }
        }

        public interface ITestService
        {
            void Test();
        }

        public class TestService : ITestService
        {
            public void Test()
            {

            }
        }

        public class MyWebSocketSession : WebSocketSession
        {
            public ITestService TestService { get; private set; }

            public MyWebSocketSession(ITestService testService)
            {
                TestService = testService;
            }
        }
    }
}
