using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace Tests
{
    public class SecureHostConfigurator : IHostConfigurator
    {
        public void Configurate(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];
                    listener.Security = SslProtocols.Tls13;
                    listener.CertificateOptions = new CertificateOptions
                    {
                        FilePath = "SuperSocket.pfx",
                        Password = "supersocket"
                    };
                });
        }

        public Stream GetClientStream(Socket socket)
        {
            return new SslStream(new NetworkStream(socket), false);
        }
    }
}