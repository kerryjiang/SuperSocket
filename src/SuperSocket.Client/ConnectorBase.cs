using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public abstract class ConnectorBase : IConnector
    {
        public IConnector NextConnector { get; private set; }

        public ConnectorBase()
        {

        }

        public ConnectorBase(IConnector nextConnector)
            : this()
        {
            NextConnector = nextConnector;
        }

        protected abstract ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken);

        async ValueTask<ConnectState> IConnector.ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var result = await ConnectAsync(remoteEndPoint, state, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return ConnectState.CancelledState;

            var nextConnector = NextConnector;

            if (!result.Result || nextConnector == null)
                return result;            

            return await nextConnector.ConnectAsync(remoteEndPoint, result, cancellationToken);
        }
    }
}