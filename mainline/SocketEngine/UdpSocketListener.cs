using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{
    class UdpSocketListener : SocketListenerBase
    {
        private Socket m_ListenSocket;

        public UdpSocketListener(ListenerInfo info)
            : base(info)
        {

        }

        public override bool Start()
        {
            try
            {
                m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                m_ListenSocket.Bind(this.EndPoint);

                var eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

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
            
        }
    }
}
