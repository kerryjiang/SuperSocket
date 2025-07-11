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

namespace SuperSocket.Tests.Http
{
    [Trait("Category", "ServerSentEvents")]
    public class ServerSentEventsTest : TestClassBase
    {
        public ServerSentEventsTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestSSEBasicEvent(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            
            using (var server = CreateSocketServerBuilder<HttpRequest, HttpKeepAliveFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    if (p.Path == "/events" && p.AcceptsEventStream)
                    {
                        var sseWriter = new ServerSentEventWriter(s.Connection);
                        await sseWriter.SendInitialResponseAsync();
                        
                        // Send a few events
                        await sseWriter.SendEventAsync("Hello SSE!", "message", "1");
                        await sseWriter.SendEventAsync("Second event", "message", "2");
                        await sseWriter.SendJsonEventAsync("{\"type\": \"notification\", \"data\": \"test\"}", "json", "3");
                        
                        await sseWriter.SendCloseEventAsync();
                    }
                    else
                    {
                        var response = new HttpResponse(404, "Not Found");
                        response.Body = "Not Found";
                        await s.SendAsync(HttpResponseEncoder.Instance, response);
                    }
                }).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                // Use raw TCP client to test SSE since HttpRequest doesn't parse SSE responses
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(IPAddress.Loopback, hostConfigurator.Listener.Port);
                    var stream = tcpClient.GetStream();

                    // Send SSE request
                    var request = "GET /events HTTP/1.1\r\n" +
                                 "Host: localhost\r\n" +
                                 "Accept: text/event-stream\r\n" +
                                 "Connection: keep-alive\r\n" +
                                 "\r\n";
                    var requestBytes = Encoding.UTF8.GetBytes(request);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    // Read response
                    var buffer = new byte[8192];
                    var totalReceived = new StringBuilder();
                    
                    // Read in chunks to get the full SSE response
                    for (int i = 0; i < 10; i++) // Limit iterations to avoid infinite loop
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            totalReceived.Append(chunk);
                            
                            // Check if we have received the close event
                            if (totalReceived.ToString().Contains("event: close"))
                                break;
                        }
                        
                        await Task.Delay(50); // Small delay between reads
                    }

                    var response = totalReceived.ToString();
                    OutputHelper.WriteLine($"SSE Response: {response}");

                    // Verify SSE response format
                    Assert.Contains("Content-Type: text/event-stream", response);
                    Assert.Contains("Cache-Control: no-cache", response);
                    Assert.Contains("Connection: keep-alive", response);
                    
                    // Verify events
                    Assert.Contains("id: 1", response);
                    Assert.Contains("event: message", response);
                    Assert.Contains("data: Hello SSE!", response);
                    
                    Assert.Contains("id: 2", response);
                    Assert.Contains("data: Second event", response);
                    
                    Assert.Contains("id: 3", response);
                    Assert.Contains("event: json", response);
                    Assert.Contains("data: {\"type\": \"notification\", \"data\": \"test\"}", response);
                    
                    Assert.Contains("event: close", response);
                    Assert.Contains("data: close", response);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        public async Task TestSSEHeartbeat(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            
            using (var server = CreateSocketServerBuilder<HttpRequest, HttpKeepAliveFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    if (p.Path == "/heartbeat" && p.AcceptsEventStream)
                    {
                        var sseWriter = new ServerSentEventWriter(s.Connection);
                        await sseWriter.SendInitialResponseAsync();
                        
                        // Send heartbeat
                        await sseWriter.SendHeartbeatAsync();
                        await sseWriter.SendEventAsync("After heartbeat", "test");
                    }
                }).BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(IPAddress.Loopback, hostConfigurator.Listener.Port);
                    var stream = tcpClient.GetStream();

                    var request = "GET /heartbeat HTTP/1.1\r\n" +
                                 "Host: localhost\r\n" +
                                 "Accept: text/event-stream\r\n" +
                                 "\r\n";
                    var requestBytes = Encoding.UTF8.GetBytes(request);
                    await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                    var buffer = new byte[4096];
                    var totalReceived = new StringBuilder();
                    
                    // Read in chunks to get the full SSE response
                    for (int i = 0; i < 10; i++) // Limit iterations to avoid infinite loop
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            totalReceived.Append(chunk);
                            
                            // Check if we have received both the heartbeat and test event
                            if (totalReceived.ToString().Contains(": heartbeat") && 
                                totalReceived.ToString().Contains("event: test"))
                                break;
                        }
                        
                        await Task.Delay(50); // Small delay between reads
                    }

                    var response = totalReceived.ToString();

                    OutputHelper.WriteLine($"Heartbeat Response: {response}");

                    // Verify heartbeat format (comment line)
                    Assert.Contains(": heartbeat", response);
                    Assert.Contains("event: test", response);
                    Assert.Contains("data: After heartbeat", response);
                }

                await server.StopAsync();
            }
        }
    }
}