using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Host;
using System.IO.Compression;

namespace SuperSocket.Tests
{
    public class GzipSecureHostConfigurator : TcpHostConfigurator
    {
        public GzipSecureHostConfigurator()
        {
            WebSocketSchema = "wss";
            IsSecure = true;
        }

        public override void Configure(ISuperSocketHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];

                    listener.AuthenticationOptions = new ServerAuthenticationOptions
                    {
                        CertificateOptions = new CertificateOptions
                            {
                                FilePath = "supersocket.pfx",
                                Password = "supersocket"
                            },
                        EnabledSslProtocols = GetServerEnabledSslProtocols()
                    };
                });
            });

            hostBuilder.UseGZip();
            base.Configure(hostBuilder);
        }
        public override async ValueTask<Stream> GetClientStream(Socket socket)
        {
            var stream = new SslStream(new DerivedNetworkStream(socket), false);
            var options = new SslClientAuthenticationOptions();
            options.TargetHost = "supersocket";
            options.EnabledSslProtocols = GetClientEnabledSslProtocols();
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            await stream.AuthenticateAsClientAsync(options);

            var zipStream = new ReadWriteDelegateStream(
                stream,
                new GZipStream(stream, CompressionMode.Decompress),
                new GZipStream(stream, CompressionMode.Compress));

            return zipStream;
        }

        protected virtual SslProtocols GetServerEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12;
        }

        protected virtual SslProtocols GetClientEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12;
        }

        public override IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
            where TPackageInfo : class
        {
            var client = new EasyClient<TPackageInfo>(pipelineFilter, options);
            client.Security = new SecurityOptions
            {
                TargetHost = "supersocket",
                EnabledSslProtocols = GetClientEnabledSslProtocols(),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            client.CompressionLevel = CompressionLevel.Optimal;
            return client;
        }
    }

}