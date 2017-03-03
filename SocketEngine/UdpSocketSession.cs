using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System.Threading;
using SuperSocket.SocketBase.Sockets;

namespace SuperSocket.SocketEngine
{
    class UdpSocketSession : SocketSession
    {
        private ISocket m_ServerSocket;
        private ISocketFactory m_SocketFactory;

        public UdpSocketSession(ISocket serverSocket, IPEndPoint remoteEndPoint, ISocketFactory socketFactory)
            : base(remoteEndPoint.ToString())
        {
            m_ServerSocket = serverSocket;
            m_SocketFactory = socketFactory;
            RemoteEndPoint = remoteEndPoint;
        }

        public UdpSocketSession(ISocket serverSocket, IPEndPoint remoteEndPoint, string sessionID, ISocketFactory socketFactory)
            : base(sessionID)
        {
            m_ServerSocket = serverSocket;
            m_SocketFactory = socketFactory;
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

        protected override void SendAsync(SendingQueue queue)
        {
            var e = m_SocketFactory.CreateSocketAsyncEventArgs();

            e.Completed += new EventHandler<ISocketAsyncEventArgs>(OnSendingCompleted);
            e.RemoteEndPoint = RemoteEndPoint;
            e.UserToken = queue;

            var item = queue[queue.Position];
            e.SetBuffer(item.Array, item.Offset, item.Count);

            if (!m_ServerSocket.SendToAsync(e))
                OnSendingCompleted(this, e);
        }

        void CleanSocketAsyncEventArgs(ISocketAsyncEventArgs e)
        {
            e.UserToken = null;
            e.Completed -= new EventHandler<ISocketAsyncEventArgs>(OnSendingCompleted);
            e.Dispose();
        }

        void OnSendingCompleted(object sender, ISocketAsyncEventArgs e)
        {
            var queue = e.UserToken as SendingQueue;

            if (e.SocketError != SocketError.Success)
            {
                var log = AppSession.Logger;

                if (log.IsErrorEnabled)
                    log.Error(new SocketException((int)e.SocketError));

                CleanSocketAsyncEventArgs(e);
                OnSendError(queue, CloseReason.SocketError);
                return;
            }

            CleanSocketAsyncEventArgs(e);

            var newPos = queue.Position + 1;

            if (newPos >= queue.Count)
            {
                OnSendingCompleted(queue);
                return;
            }

            queue.Position = newPos;
            SendAsync(queue);
        }

        protected override void SendSync(SendingQueue queue)
        {
            for (var i = 0; i < queue.Count; i++)
            {
                var item = queue[i];
                m_ServerSocket.SendTo(item.Array, item.Offset, item.Count, SocketFlags.None, RemoteEndPoint);
            }

            OnSendingCompleted(queue);
        }

        public override void ApplySecureProtocol()
        {
            throw new NotSupportedException();
        }

        protected override bool TryValidateClosedBySocket(out ISocket socket)
        {
            socket = null;
            return false;
        }

        public override int OrigReceiveOffset
        {
            get { return 0; }
        }
    }
}
