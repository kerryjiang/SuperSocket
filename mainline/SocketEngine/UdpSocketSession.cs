using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketEngine
{
    class UdpSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        private Socket m_ServerSocket;

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, ICommandReader<TCommandInfo> commandReader)
            : base(null, commandReader)
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
            IdentityKey = remoteEndPoint.ToString();
        }

        public override IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_ServerSocket.LocalEndPoint; }
        }

        public override void Start()
        {

        }

        internal void ProcessData(byte[] data)
        {
            ProcessData(data, 0, data.Length);
        }

        internal void ProcessData(byte[] data, int offset, int length)
        {
            TCommandInfo commandInfo = FindCommand(data, offset, length, false);

            if (commandInfo == null)
                return;

            ExecuteCommand(commandInfo);
        }

        public override void SendResponse(SocketContext context, string message)
        {
            byte[] data = context.Charset.GetBytes(message);
            m_ServerSocket.SendTo(data, RemoteEndPoint);
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
