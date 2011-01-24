using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Net.Sockets;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase
{
    public enum CloseReason
    {
        ServerShutdown,
        ClientClosing,
        ServerClosing,
        SocketError,
        TimeOut,
        Unknown
    }

    public class SocketSessionClosedEventArgs : EventArgs
    {
        public string IdentityKey { get; set; }
        public CloseReason Reason { get; set; }
    }

    public interface ISocketSession : ISessionBase
    {
        void Start();
        void Close(CloseReason reason);
        void SendResponse(SocketContext context, string message);
        void SendResponse(SocketContext context, byte[] data);
        void ReceiveData(Stream storeSteram, int length);
        void ReceiveData(Stream storeSteram, byte[] endMark);
        void ApplySecureProtocol(SocketContext context);
        Stream GetUnderlyStream();  
        IPEndPoint LocalEndPoint { get; }
        SslProtocols SecureProtocol { get; set; }
        event EventHandler<SocketSessionClosedEventArgs> Closed;
    }

    public interface ISocketSession<TAppSession> : ISocketSession
        where TAppSession : IAppSession, new()
    {
        void Initialize(IAppServer<TAppSession> appServer, TAppSession appSession);
    }
}
