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
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.GZip;
using SuperSocket.ProtoBase;

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
            Stream stream = new GZipReadWriteStream(new NetworkStream(socket, false), false);
            return new ValueTask<Stream>(stream);
        }

        protected virtual SslProtocols GetServerEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11;
        }

        protected virtual SslProtocols GetClientEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11;
        }

        public override IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options) where TPackageInfo : class
        {
            return new GZipEasyClient<TPackageInfo>(pipelineFilter, options);
        }
    }

}