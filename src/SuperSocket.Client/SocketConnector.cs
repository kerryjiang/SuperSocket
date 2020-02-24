using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class SocketConnector : ConnectorBase
    {
        public SocketConnector()
            : base()
        {

        }

        public SocketConnector(IConnector nextConnector)
            : base(nextConnector)
        {

        }

        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                await socket.ConnectAsync(remoteEndPoint);
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }

            return new ConnectState
            {
                Result = true,
                Socket = socket
            };            
        }
    }
}