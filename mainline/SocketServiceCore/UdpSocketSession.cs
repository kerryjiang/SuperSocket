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
        public override void Initialize(IAppServer<T> appServer, T appSession, Socket client)
        {
            base.Initialize(appServer, appSession, client);
            this.IdentityKey = client.RemoteEndPoint.ToString();
        }

        protected override void Start(SocketContext context)
        {
            throw new NotImplementedException();
        }

        public override void SendResponse(SocketContext context, string message)
        {
            
        }

        public override void SendResponse(SocketContext context, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void ApplySecureProtocol(SocketContext context)
        {
            throw new NotImplementedException();
        }

        public override void ReceiveData(Stream storeSteram, int length)
        {
            throw new NotImplementedException();
        }

        public override void ReceiveData(Stream storeSteram, byte[] endMark)
        {
            throw new NotImplementedException();
        }
    }
}
