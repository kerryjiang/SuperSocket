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
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
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
        }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        /// <value>The session ID.</value>
        public string SessionID { get; private set; }


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
            var closedHandler = Closed;
            if (closedHandler != null)
            {
                closedHandler(this, new SocketSessionClosedEventArgs
                    {
                        SessionID = this.SessionID,
                        Reason = reason
                    });
            }
        }

        /// <summary>
        /// Occurs when [closed].
        /// </summary>
        public event EventHandler<SocketSessionClosedEventArgs> Closed;

        public abstract void SendResponse(byte[] data, int offset, int length);

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
            if (Client == null && m_IsClosed)
                return;

            lock (SyncRoot)
            {
                if (Client == null && m_IsClosed)
                    return;

                Client.SafeCloseClientSocket(AppSession.Logger);

                Client = null;
                m_IsClosed = true;
                OnClose(reason);
            }
        }
    }
}
