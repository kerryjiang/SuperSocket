using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace SuperSocket.Tests
{
    public class RegularHostConfigurator : IHostConfigurator
    {
        public string WebSocketSchema => "ws";

        public bool IsSecure => false;

        public ListenOptions Listener { get; private set; }

        public void Configure(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];
                    Listener = listener;
                });
        }

        public ValueTask<Stream> GetClientStream(Socket socket)
        {
            return new ValueTask<Stream>(new DerivedNetworkStream(socket, false));
        }
    }
}