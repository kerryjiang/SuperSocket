using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// SocketFactory interface
    /// </summary>
    public interface ISocketFactory
    {
        /// <summary>
        /// Creates a new <see cref="ISocket"/>
        /// </summary>
        /// <param name="addressFamily"></param>
        /// <param name="socketType"></param>
        /// <param name="protocolType"></param>
        ISocket Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);

        /// <summary>
        /// Creates a new <see cref="ISocketAsyncEventArgs"/>
        /// </summary>
        /// <returns></returns>
        ISocketAsyncEventArgs CreateSocketAsyncEventArgs();
    }
}