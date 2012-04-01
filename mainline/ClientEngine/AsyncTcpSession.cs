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
        private SocketAsyncEventArgs m_SocketEventArgsSend;

        private byte[] m_ReceiveBuffer;

        public AsyncTcpSession(EndPoint remoteEndPoint)
            : base(remoteEndPoint)
        {

        }

        public AsyncTcpSession(EndPoint remoteEndPoint, int receiveBufferSize)
            : base(remoteEndPoint, receiveBufferSize)
        {

        }

        public override int ReceiveBufferSize
        {
            get
            {
                return base.ReceiveBufferSize;
            }

            set
            {
                if (m_ReceiveBuffer != null)
                    throw new Exception("ReceiveBufferSize cannot be set after the socket has been connected!");

                base.ReceiveBufferSize = value;
            }
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

            m_ReceiveBuffer = new byte[ReceiveBufferSize];
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
                if(!IsIgnorableSocketError((int)e.SocketError))
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
            bool raiseEvent;

            try
            {
                raiseEvent = Client.ReceiveAsync(m_SocketEventArgs);
            }
            catch (SocketException exc)
            {
                if (EnsureSocketClosed() && !IsIgnorableSocketError(exc.ErrorCode))
                    OnError(exc);

                return;
            }
            catch(Exception e)
            {
                if (EnsureSocketClosed() && !IsIgnorableException(e))
                {
                    OnError(e);
                }
                return;
            }

            if (!raiseEvent)
                ProcessReceive(m_SocketEventArgs);
        }

        protected override void SendInternal(ArraySegment<byte> segment)
        {
            if (m_SocketEventArgsSend == null)
            {
                m_SocketEventArgsSend = new SocketAsyncEventArgs();
                m_SocketEventArgsSend.Completed += new EventHandler<SocketAsyncEventArgs>(Sending_Completed);
            }

            m_SocketEventArgsSend.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool raiseEvent;

            try
            {
                raiseEvent = Client.SendAsync(m_SocketEventArgsSend);
            }
            catch (SocketException exc)
            {
                if (EnsureSocketClosed() && !IsIgnorableSocketError(exc.ErrorCode))
                    OnError(exc);

                return;
            }
            catch (Exception e)
            {
                if (EnsureSocketClosed() && IsIgnorableException(e))
                    OnError(e);
                return;
            }

            if (!raiseEvent)
                Sending_Completed(Client, m_SocketEventArgsSend);
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

                if (e.SocketError != SocketError.Success && !IsIgnorableSocketError((int)e.SocketError))
                    OnError(new SocketException((int)e.SocketError));

                return;
            }

            DequeueSend();
        }
    }
}
