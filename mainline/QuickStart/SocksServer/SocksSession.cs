using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.SocksServer
{
    public class SocksSession : AppSession<SocksSession, BinaryCommandInfo>
    {
        public new SocksServer AppServer
        {
            get
            {
                return base.AppServer as SocksServer;
            }
        }

        protected override SocketContext CreateSocketContext()
        {
            return new SocksSocketContext();
        }

        public new SocksSocketContext Context
        {
            get
            {
                return base.Context as SocksSocketContext;
            }
        }

        private Socket m_TargetSocket;

        private SocketAsyncEventArgs m_SocketEventArgs;

        internal void ConnectTargetSocket(IPEndPoint remoteEndPoint)
        {
            m_TargetSocket = new Socket(remoteEndPoint.AddressFamily, m_TargetSocket.SocketType, ProtocolType.Tcp);
            m_TargetSocket.Connect(remoteEndPoint);
            m_SocketEventArgs = AppServer.GetSocketAsyncEventArgs();
            if (m_SocketEventArgs == null)
                throw new Exception("There is no free SocketAsyncEventArgs!");
            m_SocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketAsyncEventArgs_Completed);
            SocketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(SocketSession_Closed);
        }

        void SocketSession_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            if (m_TargetSocket != null)
            {
                try
                {
                    m_TargetSocket.Shutdown(SocketShutdown.Both);
                    m_TargetSocket.Close();
                }
                catch (Exception)
                {

                }
            }

            if (m_SocketEventArgs != null)
                AppServer.ReleaseSocketAsyncEventArgs(m_SocketEventArgs);
        }

        void socketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Close(CloseReason.SocketError);
                return;
            }

            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                SendResponse(e.Buffer.CloneRange(e.Offset, e.BytesTransferred));
            }
        }

        internal void SendDataToTargetSocket(byte[] data)
        {
            try
            {
                m_TargetSocket.Send(data);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }
    }
}
