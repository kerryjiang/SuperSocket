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

        public SocketAsyncEventArgs SocketEventArgs { get; private set; }

        internal void ConnectTargetSocket(IPEndPoint remoteEndPoint)
        {
            m_TargetSocket = new Socket(remoteEndPoint.AddressFamily, m_TargetSocket.SocketType, ProtocolType.Tcp);
            m_TargetSocket.Connect(remoteEndPoint);
            SocketEventArgs = AppServer.GetSocketAsyncEventArgs();
            if (SocketEventArgs == null)
                throw new Exception("There is no free SocketAsyncEventArgs!");
            SocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketAsyncEventArgs_Completed);
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
            m_TargetSocket.Send(data);
        }
    }
}
