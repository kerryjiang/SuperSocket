using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Connection;
using SuperSocket.Server.Connection;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace SuperSocket.Tests
{
    public interface IHostConfigurator
    {
        void Configure(ISuperSocketHostBuilder hostBuilder);

        ValueTask KeepSequence();

        Socket CreateClient();

        ValueTask<Socket> CreateConnectedClientAsync();

        ValueTask<Stream> GetClientStream(Socket socket);

        TextReader GetStreamReader(Stream stream, Encoding encoding);

        string WebSocketSchema { get; }

        bool IsSecure { get; }

        ListenOptions Listener { get; }

        IEasyClient<TPackageInfo> ConfigureEasyClient<TPackageInfo>(IPipelineFilter<TPackageInfo> pipelineFilter, ConnectionOptions options)
            where TPackageInfo : class;
    }
}