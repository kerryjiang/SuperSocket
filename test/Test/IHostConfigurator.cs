using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace Tests
{
    public interface IHostConfigurator
    {
        void Configurate(HostBuilderContext context, IServiceCollection services);

        Stream GetClientStream(Socket socket);

        string WebSocketSchema { get; }
    }
}