using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore
{
    public abstract class SocketSessionBase
    {
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
        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)Client.LocalEndPoint; }
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public IPEndPoint RemoteEndPoint
        {
            get { return (IPEndPoint)Client.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets or sets the secure protocol.
        /// </summary>
        /// <value>The secure protocol.</value>
        public SslProtocols SecureProtocol { get; set; }

        /// <summary>
        /// Closes this connection.
        /// </summary>
        public virtual void Close()
        {
            if (Client != null && !m_IsClosed)
            {
                try
                {
                    Client.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e);
                }

                try
                {
                    Client.Close();
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e);
                }
                finally
                {
                    Client = null;
                    m_IsClosed = true;
                    OnClose();
                }
            }
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose()
        {
            m_IsClosed = true;
        }

        protected bool EndsWith(byte[] buffer, int offset, int length, byte[] endData)
        {
            if (length < endData.Length)
                return false;

            for (int i = 1; i <= endData.Length; i++)
            {
                if (endData[endData.Length - i] != buffer[offset + length - i])
                    return false;
            }

            return true;
        }
    }
}
