using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace SuperSocket.Tests
{
    public interface IHostConfigurator
    {
        void Configure(HostBuilderContext context, IServiceCollection services);

        ValueTask<Stream> GetClientStream(Socket socket);

        string WebSocketSchema { get; }

        bool IsSecure { get; }

        ListenOptions Listener { get; }

        IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IEasyClient<TPackageInfo> client)
            where TPackageInfo : class;
    }
}