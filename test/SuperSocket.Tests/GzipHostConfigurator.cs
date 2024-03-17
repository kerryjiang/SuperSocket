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
using System.IO.Compression;
using SuperSocket.Server.Host;

namespace SuperSocket.Tests
{
    public class GzipHostConfigurator : TcpHostConfigurator
    {
        public GzipHostConfigurator()
        {
            WebSocketSchema = "wss";
            IsSecure = false;
        }

        public override void Configure(ISuperSocketHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];

                });
            });
            hostBuilder.UseGZip();

            base.Configure(hostBuilder);
        }
        public override ValueTask<Stream> GetClientStream(Socket socket)
        {
            var networkStream = new NetworkStream(socket, false);
            var stream = new ReadWriteDelegateStream(
                networkStream,
                new GZipStream(networkStream, CompressionMode.Decompress),
                new GZipStream(networkStream, CompressionMode.Compress));
            return new ValueTask<Stream>(stream);
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
            client.CompressionLevel = CompressionLevel.Optimal;
            return client;
        }
    }

}