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
    class UdpSocketSession<TAppSession, TRequestInfo> : SocketSession<TAppSession, TRequestInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {
        private Socket m_ServerSocket;

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, IRequestFilter<TRequestInfo> requestFilter)
            : base(remoteEndPoint.ToString(), requestFilter)
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
        }

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, string sessionID)
            : base(sessionID, null)
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
        }

        public override IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)m_ServerSocket.LocalEndPoint; }
        }

        /// <summary>
        /// Updates the remote end point of the client.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point.</param>
        internal void UpdateRemoteEndPoint(IPEndPoint remoteEndPoint)
        {
            this.RemoteEndPoint = remoteEndPoint;
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

            TRequestInfo requestInfo = FindCommand(data, offset, length, isReusableBuffer, out left);

            if (requestInfo == null)
                return;

            ExecuteCommand(requestInfo);

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
            try
            {
                m_ServerSocket.SendTo(data, offset, length, SocketFlags.None, RemoteEndPoint);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }

        public override void ApplySecureProtocol()
        {
            throw new NotSupportedException();
        }
    }
}
