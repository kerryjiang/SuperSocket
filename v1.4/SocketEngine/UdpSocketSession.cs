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
            : base(commandReader)
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
            IdentityKey = remoteEndPoint.ToString();
        }

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, string sessionKey)
            : base(sessionKey, null)
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
        }

        public override IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_ServerSocket.LocalEndPoint; }
        }

        public override void Start()
        {
            StartSession();
        }

        internal void ProcessData(byte[] data)
        {
            ProcessData(data, 0, data.Length);
        }

        internal void ProcessData(byte[] data, int offset, int length)
        {
            ProcessData(data, offset, length, false);
        }

        void ProcessData(byte[] data, int offset, int length, bool isReusableBuffer)
        {
            int left;

            TCommandInfo commandInfo = FindCommand(data, offset, length, isReusableBuffer, out left);

            if (commandInfo == null)
                return;

            ExecuteCommand(commandInfo);

            if (left > 0)
            {
                ProcessData(data, offset + length - left, left, true);
            }
        }

        public override void SendResponse(string message)
        {
            byte[] data = AppSession.Charset.GetBytes(message);
            m_ServerSocket.SendTo(data, RemoteEndPoint);
        }

        public override void SendResponse(byte[] data)
        {
            m_ServerSocket.SendTo(data, RemoteEndPoint);
        }

        public override void SendResponse(byte[] data, int offset, int length)
        {
            m_ServerSocket.SendTo(data, offset, length, SocketFlags.None, RemoteEndPoint);
        }

        public override void ApplySecureProtocol()
        {
            throw new NotSupportedException();
        }
    }
}
