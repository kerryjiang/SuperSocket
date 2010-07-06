using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore
{
    public interface ISocketSession
    {        
        void Start();
        void Close();
        void SendResponse(SocketContext context, string message);
        void InitStream(SocketContext context);
        string SessionID { get; }
        DateTime LastActiveTime { get; }
        IPEndPoint LocalEndPoint { get; }
        SslProtocols SecureProtocol { get; set; }
        //IServerConfig Config { get;  set; }        
        event EventHandler<SocketSessionClosedEventArgs> Closed;
    }

    public interface ISocketSession<T> : ISocketSession
        where T : IAppSession, new()
    {
        void Initialize(IAppServer<T> appServer, T appSession, Socket client);
    }
}
