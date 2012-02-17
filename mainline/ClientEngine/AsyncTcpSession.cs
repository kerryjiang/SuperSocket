using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public class AsyncTcpSession : TcpClientSession
    {
        private SocketAsyncEventArgs m_SocketEventArgs;

        private byte[] m_ReceiveBuffer;

        public AsyncTcpSession(EndPoint remoteEndPoint)
            : base(remoteEndPoint)
        {
        }

        protected override void SocketEventArgsCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                ProcessConnect(e);
                return;
            }

            ProcessReceive(e);
        }

        protected override void StartReceive(SocketAsyncEventArgs e)
        {
            OnConnected();

            int receiveBufferSize = 1024;
            m_ReceiveBuffer = new byte[receiveBufferSize];
            e.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);
            m_SocketEventArgs = e;

            StartReceive();
        }

        private void BeginReceive()
        {
            if (!Client.ReceiveAsync(m_SocketEventArgs))
                ProcessReceive(m_SocketEventArgs);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                if(EnsureSocketClosed())
                    OnClosed();
                OnError(new SocketException((int)e.SocketError));
                return;
            }

            if (e.BytesTransferred == 0)
            {
                if(EnsureSocketClosed())
                    OnClosed();
                return;
            }

            OnDataReceived(e.Buffer, e.Offset, e.BytesTransferred);
            StartReceive();
        }

        void StartReceive()
        {
            bool async;

            try
            {
                async = Client.ReceiveAsync(m_SocketEventArgs);
            }
            catch(Exception e)
            {
                if(EnsureSocketClosed())
                    OnError(e);
                return;
            }

            if (!async)
                ProcessReceive(m_SocketEventArgs);
        }

        protected override void SendInternal(ArraySegment<byte> segment)
        {
            var args = new SocketAsyncEventArgs();

            args.SetBuffer(segment.Array, segment.Offset, segment.Count);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Sending_Completed);

            bool async;

            try
            {
                async = Client.SendAsync(args);
            }
            catch (Exception e)
            {
                if (EnsureSocketClosed())
                    OnError(e);
                return;
            }

            if (!async)
                Sending_Completed(Client, args);
        }

        void Sending_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send)
            {
                IsSending = false;
                return;
            }

            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                IsSending = false;
                if(EnsureSocketClosed())
                    OnClosed();

                if (e.SocketError != SocketError.Success)
                    OnError(new SocketException((int)e.SocketError));

                return;
            }

            DequeueSend();
        }
    }
}
