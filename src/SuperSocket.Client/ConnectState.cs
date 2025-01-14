using System;
using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.ObjectPool;
using SuperSocket.Connection;

namespace SuperSocket.Client
{
    public class ConnectState
    {
        public ConnectState()
        {

        }

        private ConnectState(bool cancelled)
        {
            Cancelled = cancelled;
        }

        public bool Result { get; set; }

        public bool Cancelled { get; private set; }

        public Exception Exception { get; set; }

        public Socket Socket { get; set; }

        public Stream Stream { get; set; }

        public static readonly ConnectState CancelledState = new ConnectState(false);

        private static Lazy<ObjectPool<SocketSender>> _socketSenderPool = new Lazy<ObjectPool<SocketSender>>(() =>
        {
            var policy = new DefaultPooledObjectPolicy<SocketSender>();
            var pool = new DefaultObjectPool<SocketSender>(policy, EasyClient.SocketSenderPoolSzie ?? EasyClient.DefaultSocketSenderPoolSzie);
            return pool;
        });

        public IConnection CreateConnection(ConnectionOptions connectionOptions)
        {
            var stream = this.Stream;
            var socket = this.Socket;

            if (stream != null)
            {
                return new StreamPipeConnection(stream , socket.RemoteEndPoint, socket.LocalEndPoint, connectionOptions);
            }
            else
            {
                return new TcpPipeConnection(socket, connectionOptions, _socketSenderPool.Value);
            }
        }
    }
}