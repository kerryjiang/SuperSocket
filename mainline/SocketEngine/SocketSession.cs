using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Socket Session, all application session should base on this class
    /// </summary>
    abstract class SocketSession : ISocketSession
    {
        public IAppSession AppSession { get; private set; }

        protected readonly object SyncRoot = new object();

        private bool m_InSending = false;

        protected bool SyncSend { get; private set; }

        public SocketSession(Socket client)
            : this(Guid.NewGuid().ToString())
        {
            if (client == null)
                throw new ArgumentNullException("client");

            Client = client;
            LocalEndPoint = (IPEndPoint)client.LocalEndPoint;
            RemoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        }

        public SocketSession(string sessionID)
        {
            SessionID = sessionID;
        }

        public virtual void Initialize(IAppSession appSession)
        {
            AppSession = appSession;
            SyncSend = appSession.Config.SyncSend;
        }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public string SessionID { get; private set; }


        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public IServerConfig Config { get; set; }

        /// <summary>
        /// Starts this session.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Says the welcome information when a client connectted.
        /// </summary>
        protected virtual void StartSession()
        {
            AppSession.StartSession();
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose(CloseReason reason)
        {
            m_IsClosed = true;

            if (m_InSending)
                m_InSending = false;

            var closedHandler = Closed;
            if (closedHandler != null)
            {
                closedHandler(this, reason);
            }
        }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public Action<ISocketSession, CloseReason> Closed { get; set; }

        /// <summary>
        /// Starts the sending.
        /// </summary>
        public void StartSend()
        {
            if(m_InSending)
                return;

            lock (SyncRoot)
            {
                if (m_InSending)
                    return;

                ArraySegment<byte> data;

                if (AppSession.TryGetSendingData(out data))
                {
                    m_InSending = true;
                    SendResponse(data.Array, data.Offset, data.Count);
                }
            }
        }

        protected abstract void SendAsync(byte[] data, int offset, int length);

        protected abstract void SendSync(byte[] data, int offset, int length);

        private void SendResponse(byte[] data, int offset, int length)
        {
            if (SyncSend)
            {
                SendSync(data, offset, length);
            }
            else
            {
                SendAsync(data, offset, length);
            }
        }

        protected void OnSendingCompleted()
        {
            ArraySegment<byte> data;

            if (AppSession.TryGetSendingData(out data))
            {
                SendResponse(data.Array, data.Offset, data.Count);
            }
            else
            {
                m_InSending = false;
            }
        }

        public abstract void ApplySecureProtocol();

        public Stream GetUnderlyStream()
        {
            return new NetworkStream(Client);
        }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        public Socket Client { get; set; }

        private bool m_IsClosed = false;

        protected bool IsClosed
        {
            get { return m_IsClosed; }
        }

        /// <summary>
        /// Gets the local end point.
        /// </summary>
        /// <value>The local end point.</value>
        public virtual IPEndPoint LocalEndPoint { get; protected set; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public virtual IPEndPoint RemoteEndPoint { get; protected set; }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>The secure protocol.</value>
        public SslProtocols SecureProtocol { get; set; }

        public virtual void Close(CloseReason reason)
        {
            var client = Client;

            if (client == null && m_IsClosed)
                return;

            lock (SyncRoot)
            {
                if (client == null && m_IsClosed)
                    return;

                client.SafeClose();
                Client = null;
                OnClose(reason);
            }
        }
    }
}
