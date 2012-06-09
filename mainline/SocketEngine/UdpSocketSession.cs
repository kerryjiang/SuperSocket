using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    class UdpSocketSession : SocketSession
    {
        private Socket m_ServerSocket;

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint)
            : base(remoteEndPoint.ToString())
        {
            m_ServerSocket = serverSocket;
            RemoteEndPoint = remoteEndPoint;
        }

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint, string sessionID)
            : base(sessionID)
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

        protected override void SendAsync(byte[] data, int offset, int length)
        {
            var e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(SendingCompleted);
            e.RemoteEndPoint = RemoteEndPoint;
            e.SetBuffer(data, offset, length);
            m_ServerSocket.SendToAsync(e);
        }

        void SendingCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                var log = AppSession.Logger;

                if (log.IsErrorEnabled)
                    log.Error(new SocketException((int)e.SocketError));

                return;
            }

            OnSendingCompleted();
        }

        protected override void SendSync(byte[] data, int offset, int length)
        {
            m_ServerSocket.SendTo(data, offset, length, SocketFlags.None, RemoteEndPoint);
            OnSendingCompleted();
        }

        public override void ApplySecureProtocol()
        {
            throw new NotSupportedException();
        }
    }
}
