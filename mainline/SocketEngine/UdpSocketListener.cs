using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    class UdpSocketListener : SocketListenerBase
    {
        private Socket m_ListenSocket;

        public UdpSocketListener(ListenerInfo info)
            : base(info)
        {

        }

        /// <summary>
        /// Starts to listen
        /// </summary>
        /// <param name="config">The server config.</param>
        /// <returns></returns>
        public override bool Start(IServerConfig config)
        {
            try
            {
                m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                m_ListenSocket.Bind(this.EndPoint);

                var eventArgs = new SocketAsyncEventArgs();

                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                int receiveBufferSize = config.ReceiveBufferSize <= 0 ? 2048 : config.ReceiveBufferSize;
                var buffer = new byte[receiveBufferSize];
                eventArgs.SetBuffer(buffer, 0, buffer.Length);

                m_ListenSocket.ReceiveFromAsync(eventArgs);

                return true;
            }
            catch (Exception e)
            {
                OnError(e);
                return false;
            }
        }

        void eventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                OnError(new SocketException((int)e.SocketError));
                return;
            }

            if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                try
                {
                    OnNewClientAccepted(m_ListenSocket, new object[] { e.Buffer.CloneRange(e.Offset, e.BytesTransferred), e.RemoteEndPoint.Serialize() });
                    m_ListenSocket.ReceiveFromAsync(e);
                }
                catch (Exception exc)
                {
                    OnError(exc);
                }
            }
        }

        public override void Stop()
        {
            if (m_ListenSocket == null)
                return;

            lock(this)
            {
                if (m_ListenSocket == null)
                    return;

                try
                {
                    m_ListenSocket.Shutdown(SocketShutdown.Both);
                    m_ListenSocket.Close();
                }
                finally
                {
                    m_ListenSocket = null;
                }
            }
        }
    }
}
