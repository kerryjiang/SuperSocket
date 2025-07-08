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
using SuperSocket.Http;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using SuperSocket.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.Connection;
using SuperSocket.Server.Host;
using System.Threading;

namespace SuperSocket.Tests
{
    [Trait("Category", "HttpKeepAlive")]
    public class HttpKeepAliveTest : TestClassBase
    {
        public HttpKeepAliveTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestKeepAliveConnection(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            var requestCount = 0;
            
            using (var server = CreateSocketServerBuilder<HttpRequest, HttpKeepAliveFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    requestCount++;
                    
                    var response = new HttpResponse();
                    response.Body = $"Response {requestCount}: {p.Path}";
                    response.KeepAlive = p.KeepAlive;
                    
                    await s.SendAsync(response.ToBytes());
                    
                    // Close the session when KeepAlive is false
                    if (!response.KeepAlive)
                    {
                        await s.CloseAsync(CloseReason.LocalClosing);
                    }
                }).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var services = new ServiceCollection();
                services.AddLogging();
                services.Configure<ILoggingBuilder>((loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                });

                var sp = services.BuildServiceProvider();
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Client");

                using (var client = new EasyClient<HttpRequest>(new HttpKeepAliveFilter(), logger).AsClient())
                {
                    var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                    Assert.True(connected);

                    // Send multiple requests over the same connection
                    for (int i = 1; i <= 3; i++)
                    {
                        var request = $"GET /test{i} HTTP/1.1\r\nHost: localhost\r\nConnection: keep-alive\r\n\r\n";
                        await client.SendAsync(Encoding.ASCII.GetBytes(request));

                        var response = await client.ReceiveAsync();
                        Assert.NotNull(response);
                        Assert.Contains($"Response {i}: /test{i}", response.Body);
                        Assert.True(response.KeepAlive);
                    }

                    // Send a final request to close the connection
                    var closeRequest = "GET /close HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n";
                    await client.SendAsync(Encoding.ASCII.GetBytes(closeRequest));

                    var closeResponse = await client.ReceiveAsync();
                    Assert.NotNull(closeResponse);
                    Assert.Contains("Response 4: /close", closeResponse.Body);
                    Assert.False(closeResponse.KeepAlive);

                    await client.CloseAsync();
                }

                Assert.Equal(4, requestCount);
                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestHttpResponse(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            
            using (var server = CreateSocketServerBuilder<HttpRequest, HttpPipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    // Create response using HttpResponse class
                    var response = new HttpResponse(200, "OK");
                    response.SetContentType("application/json");
                    response.Body = "{\"message\": \"Hello World\"}";
                    response.KeepAlive = false; // Ensure connection closes

                    // Convert to bytes and send
                    await s.SendAsync(response.ToBytes());
                    
                    // Close the session when KeepAlive is false
                    if (!response.KeepAlive)
                    {
                        await s.CloseAsync(CloseReason.LocalClosing);
                    }
                }).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var services = new ServiceCollection();
                services.AddLogging();
                services.Configure<ILoggingBuilder>((loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                });

                var sp = services.BuildServiceProvider();
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Client");

                using (var client = new EasyClient<HttpRequest>(new HttpPipelineFilter(), logger).AsClient())
                {
                    var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                    Assert.True(connected);

                    var request = "GET /api/test HTTP/1.1\r\nHost: localhost\r\n\r\n";
                    await client.SendAsync(Encoding.ASCII.GetBytes(request));

                    var response = await client.ReceiveAsync();
                    Assert.NotNull(response);
                    Assert.Equal("{\"message\": \"Hello World\"}", response.Body);

                    await client.CloseAsync();
                }

                await server.StopAsync();
            }
        }
    }
}