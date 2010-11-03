using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace SuperSocket.SocketServiceCore
{
    class UdpSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
    {
        public override IPEndPoint LocalEndPoint
        {
            get { throw new NotSupportedException(); }
        }

        private IPEndPoint m_RemoteEndPoint;

        public override IPEndPoint RemoteEndPoint
        {
            get { return m_RemoteEndPoint; }
        }

        protected override void Start(SocketContext context)
        {
            m_RemoteEndPoint = context.DataContext as IPEndPoint;
            IdentityKey = RemoteEndPoint.ToString();
        }        

        public override void SendResponse(SocketContext context, string message)
        {
            
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            throw new NotSupportedException();
        }

        public override void ApplySecureProtocol(SocketContext context)
        {
            throw new NotSupportedException();
        }

        public override void ReceiveData(Stream storeSteram, int length)
        {
            throw new NotSupportedException();
        }

        public override void ReceiveData(Stream storeSteram, byte[] endMark)
        {
            throw new NotSupportedException();
        }
    }
}
