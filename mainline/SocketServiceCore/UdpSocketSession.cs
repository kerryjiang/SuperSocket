using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore
{
    class UdpSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
    {
        private Socket m_ServerSocket;

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint)
            : base()
        {
            m_ServerSocket = serverSocket;
            m_RemoteEndPoint = remoteEndPoint;
            IdentityKey = m_RemoteEndPoint.ToString();
        }

        public override IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_ServerSocket.LocalEndPoint; }
        }

        private readonly IPEndPoint m_RemoteEndPoint;

        public override IPEndPoint RemoteEndPoint
        {
            get { return m_RemoteEndPoint; }
        }

        protected override void Start(SocketContext context)
        {
            
        }

        public override void SendResponse(SocketContext context, string message)
        {
            LogUtil.LogInfo("Server prepare sent: " + message);
            byte[] data = context.Charset.GetBytes(message);
            m_ServerSocket.SendTo(data, m_RemoteEndPoint);
            LogUtil.LogInfo("Server sent: " + message);
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            throw new NotSupportedException();
        }

        public override void ApplySecureProtocol(SocketContext context)
        {
            throw new NotSupportedException();
        }

        public override void ReceiveData(Stream storeSteram, int length)
        {
            throw new NotSupportedException();
        }

        public override void ReceiveData(Stream storeSteram, byte[] endMark)
        {
            throw new NotSupportedException();
        }
    }
}
