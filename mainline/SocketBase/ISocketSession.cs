using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Net.Sockets;

namespace SuperSocket.SocketBase
{
    public class SocketSessionClosedEventArgs : EventArgs
    {
        public string IdentityKey { get; set; }
    }

    public interface ISocketSession
    {
        void Start();
        void Close();
        void SendResponse(SocketContext context, string message);
        void SendResponse(SocketContext context, byte[] data);
        void ReceiveData(Stream storeSteram, int length);
        void ReceiveData(Stream storeSteram, byte[] endMark);
        void ApplySecureProtocol(SocketContext context);
        Stream GetUnderlyStream();
        string SessionID { get; }
        string IdentityKey { get; }        
        IPEndPoint LocalEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }
        SslProtocols SecureProtocol { get; set; }
        event EventHandler<SocketSessionClosedEventArgs> Closed;
    }

    public interface ISocketSession<T> : ISocketSession
        where T : IAppSession, new()
    {
        void Initialize(IAppServer<T> appServer, T appSession, Socket client);
    }
}
