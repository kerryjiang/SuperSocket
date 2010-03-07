using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketServiceCore.Config;
using System.Security.Authentication;
using System.Net;

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
        event EventHandler Closed;
    }

    public interface ISocketSession<T> : ISocketSession
        where T : IAppSession, new()
    {
        void Initialize(IAppServer<T> appServer, T appSession, TcpClient client);
    }
}
