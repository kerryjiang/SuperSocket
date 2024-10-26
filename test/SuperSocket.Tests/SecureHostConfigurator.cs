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

namespace SuperSocket.Tests
{
    public class SecureHostConfigurator : TcpHostConfigurator
    {
        private SslProtocols _currentSslProtocols;

        public SecureHostConfigurator()
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

                    var authenticationOptions = listener.AuthenticationOptions;

                    if (authenticationOptions == null)
                    {
                        authenticationOptions = listener.AuthenticationOptions = new ServerAuthenticationOptions();
                    }

                    authenticationOptions.CertificateOptions = new CertificateOptions
                    {
                        FilePath = "supersocket.pfx",
                        Password = "supersocket"
                    };

                    if (authenticationOptions.EnabledSslProtocols == SslProtocols.None)
                    {
                        authenticationOptions.EnabledSslProtocols = GetServerEnabledSslProtocols();
                    }

                    _currentSslProtocols = authenticationOptions.EnabledSslProtocols;
                });
            });

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
            return stream;
        }

        protected virtual SslProtocols GetServerEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12;
        }

        protected virtual SslProtocols GetClientEnabledSslProtocols()
        {
            return _currentSslProtocols;
        }

        public override IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
            where TPackageInfo : class
        {
            var client =  new EasyClient<TPackageInfo>(pipelineFilter, options);
            client.Security = new SecurityOptions
            {
                TargetHost = "supersocket",
                EnabledSslProtocols = GetClientEnabledSslProtocols(),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            return client;
        }
    }

    public class TLS13OnlySecureHostConfigurator : SecureHostConfigurator
    {
        protected override SslProtocols GetServerEnabledSslProtocols()
        {
            return SslProtocols.Tls13;
        }

        protected override SslProtocols GetClientEnabledSslProtocols()
        {
            return SslProtocols.Tls13;
        }
    }
}