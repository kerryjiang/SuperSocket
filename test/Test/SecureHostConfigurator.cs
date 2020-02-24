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
using SuperSocket.ProtoBase;

namespace Tests
{
    public class SecureHostConfigurator : IHostConfigurator
    {
        public string WebSocketSchema => "wss";

        public bool IsSecure => true;

        public ListenOptions Listener { get; private set; }

        public void Configurate(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];
                    listener.Security = SslProtocols.Tls13 | SslProtocols.Tls12;
                    listener.CertificateOptions = new CertificateOptions
                    {
                        FilePath = "supersocket.pfx",
                        Password = "supersocket"
                    };
                    Listener = listener;
                });
        }

        public async ValueTask<Stream> GetClientStream(Socket socket)
        {
            var stream = new SslStream(new NetworkStream(socket), false);
            var options = new SslClientAuthenticationOptions();
            options.TargetHost = "supersocket";
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            await stream.AuthenticateAsClientAsync(options);
            return stream;
        }
    }
}