using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace Tests
{
    public class RegularHostConfigurator : IHostConfigurator
    {
        public void Configurate(HostBuilderContext context, IServiceCollection services)
        {
            // do nothing
        }

        public Stream GetClientStream(Socket socket)
        {
            return new NetworkStream(socket);
        }
    }
}