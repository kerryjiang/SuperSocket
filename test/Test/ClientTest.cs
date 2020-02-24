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
using SuperSocket.Client;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Tests
{
    [Trait("Category", "Client")]
    public class ClientTest : TestClassBase
    {
        public ClientTest(ITestOutputHelper outputHelper)
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

            protected override IConnector GetConntector()
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
            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .ConfigurePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer())
            {

                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");                

                EasyClient<TextPackageInfo> client;
                
                if (hostConfigurator.IsSecure)
                    client = new SecureEasyClient<TextPackageInfo>(new LinePipelineFilter());
                else
                    client = new EasyClient<TextPackageInfo>(new LinePipelineFilter());

                var connected = await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, hostConfigurator.Listener.Port));
                
                Assert.True(connected);

                for (var i = 0; i < 10; i++)
                {
                    var msg = Guid.NewGuid().ToString();
                    await client.SendAsync(Utf8Encoding.GetBytes(msg + "\r\n"));
                    var package = await client.ReceiveAsync();
                    Assert.NotNull(package);
                    Assert.Equal(msg, package.Text);
                }

                client.Close();

                await server.StopAsync();
            }
        }
    }
}
