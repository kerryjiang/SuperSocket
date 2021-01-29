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
using Xunit.Abstractions;
using SuperSocket.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SuperSocket.Channel;

namespace SuperSocket.Tests
{
    [Trait("Category", "Http")]
    public class HttpPipelineFilterTest : TestClassBase
    {
        public HttpPipelineFilterTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        class SecureEasyClient<TReceivePackage> : EasyClient<TReceivePackage>
            where TReceivePackage : class
        {
            public SecureEasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger = null)
                : base(pipelineFilter, logger)
            {

            }

            protected override IConnector GetConnector()
            {
                var authOptions = new SslClientAuthenticationOptions();
                authOptions.EnabledSslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12;
                authOptions.TargetHost = IPAddress.Loopback.ToString();
                authOptions.RemoteCertificateValidationCallback += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    return true;
                };

                return new SocketConnector(new SslStreamConnector(authOptions));
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestEcho(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            using (var server = CreateSocketServerBuilder<HttpRequest, HttpPipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    var response = "HTTP/1.1 200 OK\r\n"
                        + "Date: Sat, 09 Oct 2010 14:28:02 GMT\r\n"
                        + "Server: Apache\r\n"
                        + "Last-Modified: Tue, 01 Dec 2009 20:18:22 GMT\r\n"
                        + "ETag: \"51142bc1-7449-479b075b2891b\"\r\n"
                        + "Accept-Ranges: bytes\r\n"
                        + "Content-Length: 12\r\n"
                        + "Content-Type: text/html\r\n\r\nHello World!";

                    await s.SendAsync(Utf8Encoding.GetBytes(response));
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");                

                IEasyClient<HttpRequest> client;

                var services = new ServiceCollection();
                services.AddLogging();
                services.Configure<ILoggingBuilder>((loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                });

                var sp = services.BuildServiceProvider();

                var loggerFactory = sp.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Client");
                
                if (hostConfigurator.IsSecure)
                    client = (new SecureEasyClient<HttpRequest>(new HttpPipelineFilter(), logger)).AsClient();
                else
                    client = new EasyClient<HttpRequest>(new HttpPipelineFilter(), logger).AsClient();

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                var request = "GET /supersocket HTTP/1.1\r\nHost: developer.mozilla.org\r\nAccept-Language: fr\r\n\r\n";
                var data = Encoding.ASCII.GetBytes(request);
                await client.SendAsync(Encoding.ASCII.GetBytes(request));

                var response = await client.ReceiveAsync();

                Assert.Equal("Hello World!", response.Body);

                await client.CloseAsync();
                await server.StopAsync();
            }
        }
    }
}
