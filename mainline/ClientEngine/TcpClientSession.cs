using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;

namespace SuperSocket.ClientEngine
{
    public class TcpClientSession<TCommandInfo, TContext> : ClientSession<TCommandInfo, TContext>
        where TCommandInfo : ICommandInfo
        where TContext : class
    {
        private SocketAsyncEventArgs m_ReceiveEventArgs;

        private byte[] m_ReceiveBuffer;

        public TcpClientSession()
        {

        }

        public override void Initialize(NameValueCollection settings, IClientCommandReader<TCommandInfo> commandReader, IEnumerable<Assembly> commandAssemblies)
        {
            base.Initialize(settings, commandReader, commandAssemblies);

            int receiveBufferSize = 1024;
            m_ReceiveEventArgs = new SocketAsyncEventArgs();
            m_ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_ReceiveEventArgs_Completed);
            m_ReceiveBuffer = new byte[receiveBufferSize];
            m_ReceiveEventArgs.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);
        }

        public override void Connect(IPEndPoint remoteEndPoint)
        {
            if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, m_ReceiveEventArgs))
                ProcessAccept(m_ReceiveEventArgs);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Client = e.ConnectSocket;
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

            int offset = e.Offset;
            int length = e.BytesTransferred;

            while (true)
            {
                int left;

                var commandInfo = CommandReader.GetCommandInfo(e.Buffer, offset, length, out left);

                if (commandInfo != null)
                {
                    ExecuteCommand(commandInfo);

                    if (left <= 0)
                        break;

                    offset = e.Offset + e.BytesTransferred - left;
                    continue;
                }

                break;
            }

            StartReceive();
        }


        void EnsureSocketClosed()
        {
            if (Client != null)
            {
                if (Client.Connected)
                {
                    Client.Shutdown(SocketShutdown.Both);
                    Client.Close();
                }

                Client = null;
            }
        }

        void StartReceive()
        {
            if (!Client.ReceiveAsync(m_ReceiveEventArgs))
                ProcessReceive(m_ReceiveEventArgs);
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
