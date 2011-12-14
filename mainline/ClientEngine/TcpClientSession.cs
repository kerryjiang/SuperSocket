using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public abstract class TcpClientSession : ClientSession
    {
        private SocketAsyncEventArgs m_SocketEventArgs;

        private byte[] m_ReceiveBuffer;

        public TcpClientSession()
            : base()
        {
            Init();
        }

        public TcpClientSession(EndPoint remoteEndPoint)
            : base(remoteEndPoint)
        {
            Init();
        }

        private void Init()
        {
            m_SocketEventArgs = new SocketAsyncEventArgs();
            m_SocketEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_SocketEventArgs_Completed);
        }

        void m_SocketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                ProcessAccept(e);
                return;
            }

            ProcessReceive(e);
        }

        protected override void Connect()
        {
            var client = new Socket(RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            m_SocketEventArgs.RemoteEndPoint = RemoteEndPoint;
            m_SocketEventArgs.UserToken = client;
            
            if(!client.ConnectAsync(m_SocketEventArgs))
                ProcessAccept(m_SocketEventArgs);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Client = e.UserToken as Socket;
            e.UserToken = null;
            
            int receiveBufferSize = 1024;
            m_ReceiveBuffer = new byte[receiveBufferSize];
            m_SocketEventArgs.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);

            StartReceive();
            OnConnected();
        }

        private void BeginReceive()
        {
            if (!Client.ReceiveAsync(m_SocketEventArgs))
                ProcessReceive(m_SocketEventArgs);
        }

        protected virtual void OnConnected()
        {

        }

        public override void Close()
        {
            EnsureSocketClosed();
        }

        void m_ReceiveEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                ProcessAccept(e);
                return;
            }

            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                EnsureSocketClosed();
                OnClosed();
                return;
            }

            if (e.BytesTransferred == 0)
            {
                EnsureSocketClosed();
                OnClosed();
                return;
            }

            OnDataReceived(e.Buffer, e.Offset, e.BytesTransferred);
            StartReceive();
        }

        protected abstract void OnDataReceived(byte[] data, int offset, int length);

        void EnsureSocketClosed()
        {
            if (Client != null)
            {
                if (Client.Connected)
                {
                    try
                    {
                        Client.Shutdown(SocketShutdown.Both);
                        Client.Close();
                    }
                    catch
                    {

                    }
                }

                Client = null;
            }
        }

        void StartReceive()
        {
            bool async;

            try
            {
                async = Client.ReceiveAsync(m_SocketEventArgs);
            }
            catch
            {
                EnsureSocketClosed();
                return;
            }

            if (!async)
                ProcessReceive(m_SocketEventArgs);
        }

        private ConcurrentQueue<ArraySegment<byte>> m_SendingQueue = new ConcurrentQueue<ArraySegment<byte>>();

        private volatile bool m_IsSending = false;

        public override void Send(byte[] data, int offset, int length)
        {
            m_SendingQueue.Enqueue(new ArraySegment<byte>(data, offset, length));

            if (!m_IsSending)
            {
                DequeueSend();
            }
        }

        private void DequeueSend()
        {
            m_IsSending = true;
            ArraySegment<byte> segment;

            if (!m_SendingQueue.TryDequeue(out segment))
            {
                m_IsSending = false;
                return;
            }

            var args = new SocketAsyncEventArgs();

            args.SetBuffer(segment.Array, segment.Offset, segment.Count);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Sending_Completed);

            bool async;

            try
            {
                async = Client.SendAsync(args);
            }
            catch
            {
                EnsureSocketClosed();
                return;
            }

            if (!async)
                Sending_Completed(Client, args);
        }

        void Sending_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send)
            {
                m_IsSending = false;
                return;
            }

            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                m_IsSending = false;
                EnsureSocketClosed();
                OnClosed();
                return;
            }

            DequeueSend();
        }
    }
}
