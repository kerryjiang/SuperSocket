using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// Passthrough Socket Factory
    /// </summary>
    public class PassthroughSocketFactory : ISocketFactory
    {
        /// <inheritdoc />
        public ISocket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new PassthroughSocket(addressFamily, socketType, protocolType);
        }

        /// <inheritdoc />
        public ISocketAsyncEventArgs CreateSocketAsyncEventArgs()
        {
            return new PassthroughSocketAsyncEventArgs();
        }
    }
}