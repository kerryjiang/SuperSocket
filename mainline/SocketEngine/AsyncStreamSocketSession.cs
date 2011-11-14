using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using System.Net.Sockets;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    class AsyncStreamSocketSession<TAppSession, TCommandInfo> : SocketSession<TAppSession, TCommandInfo>, IAsyncSocketSession
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public AsyncStreamSocketSession(Socket client, ICommandReader<TCommandInfo> initialCommandReader)
            : base(client, initialCommandReader)
        {
            
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void SendResponse(string message)
        {
            throw new NotImplementedException();
        }

        public override void SendResponse(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void SendResponse(byte[] data, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override void ApplySecureProtocol()
        {
            throw new NotImplementedException();
        }

        public override void ReceiveData(System.IO.Stream storeSteram, int length)
        {
            throw new NotImplementedException();
        }

        public override void ReceiveData(System.IO.Stream storeSteram, byte[] endMark)
        {
            throw new NotImplementedException();
        }

        public AsyncSocket.SocketAsyncEventArgsProxy SocketAsyncProxy
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void ProcessReceive(System.Net.Sockets.SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        public Common.ILogger Logger
        {
            get { throw new NotImplementedException(); }
        }
    }
}
