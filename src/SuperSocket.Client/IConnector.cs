using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public interface IConnector
    {
        ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state = null, CancellationToken cancellationToken = default);

        IConnector NextConnector { get; }
    }
}