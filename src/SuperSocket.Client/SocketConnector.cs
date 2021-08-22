using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SocketConnector : ConnectorBase
    {
        public IPEndPoint LocalEndPoint { get; private set; }

        public SocketConnector()
            : base()
        {

        }

        public SocketConnector(IConnector nextConnector)
            : base(nextConnector)
        {

        }

        public SocketConnector(IPEndPoint localEndPoint)
            : base()
        {
            LocalEndPoint = localEndPoint;
        }

        public SocketConnector(IPEndPoint localEndPoint, IConnector nextConnector)
            : base(nextConnector)
        {
            LocalEndPoint = localEndPoint;
        }

        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                var localEndPoint = LocalEndPoint;

                if (localEndPoint != null)
                {
                    socket.ExclusiveAddressUse = false;
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                    socket.Bind(localEndPoint);
                }

                await socket.ConnectAsync(remoteEndPoint);
                return new ConnectState
                {
                    Result = true,
                    Socket = socket,
                    Stream = new NetworkStream(socket, System.IO.FileAccess.ReadWrite, true)
                };
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

        }
    }
}