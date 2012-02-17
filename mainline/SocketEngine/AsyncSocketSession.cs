using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketSession : SocketSession, IAsyncSocketSession
    {        
        private AsyncSocketSender m_AsyncSender;

        private bool m_IsReset;

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy socketAsyncProxy)
            : this(client, socketAsyncProxy, false)
        {

        }

        public AsyncSocketSession(Socket client, SocketAsyncEventArgsProxy socketAsyncProxy, bool isReset)
            : base(client)
        {
            m_AsyncSender = new AsyncSocketSender(client);
            SocketAsyncProxy = socketAsyncProxy;
            m_IsReset = isReset;
        }

        ILog IAsyncSocketSessionBase.Logger
        {
            get { return AppSession.Logger; }
        }

        public override void Start()
        {
            SocketAsyncProxy.Initialize(this);
            StartReceive(SocketAsyncProxy.SocketEventArgs);

            if (!m_IsReset)
                StartSession();
        }

        private void StartReceive(SocketAsyncEventArgs e)
        {
            if (IsClosed)
                return;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = Client.ReceiveAsync(e);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
                return;
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        public override void SendResponse(byte[] data, int offset, int length)
        {
            try
            {
                m_AsyncSender.Send(data, offset, length);
            }
            catch (Exception)
            {
                Close(CloseReason.SocketError);
            }
        }

        public SocketAsyncEventArgsProxy SocketAsyncProxy { get; private set; }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            if (e.BytesTransferred <= 0)
            {
                Close(CloseReason.ClientClosing);
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                Close(CloseReason.SocketError);
                return;
            }

            int bytesTransferred = e.BytesTransferred;
            int offset = e.Offset;

            this.AppSession.ProcessRequest(e.Buffer, e.Offset, e.BytesTransferred, true);

            //read the next block of data sent from the client
            StartReceive(e);
        }      

        public override void ApplySecureProtocol()
        {
            //TODO: Implement async socket SSL/TLS encryption
        }
    }
}
