using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketServiceCore.AsyncSocket;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore
{
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
        DateTime LastActiveTime { get; }
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

    interface IAsyncSocketSession
    {
        SocketAsyncEventArgsProxy SocketAsyncProxy { get; set; }

        void ProcessReceive(SocketAsyncEventArgs e);
    }
}
