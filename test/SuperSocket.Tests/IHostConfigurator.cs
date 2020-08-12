using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;

namespace SuperSocket.Tests
{
    public interface IHostConfigurator
    {
        void Configurate(HostBuilderContext context, IServiceCollection services);

        ValueTask<Stream> GetClientStream(Socket socket);

        string WebSocketSchema { get; }

        bool IsSecure { get; }

        ListenOptions Listener { get; }
    }
}