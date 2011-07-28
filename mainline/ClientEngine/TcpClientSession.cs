using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase.Command;
using System.Collections.Specialized;
using System.Net;

namespace SuperSocket.ClientEngine
{
    class TcpClientSession<TCommandInfo> : ClientSession<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        private SocketAsyncEventArgs m_ReceiveEventArgs;

        private byte[] m_ReceiveBuffer;

        public TcpClientSession()
        {

        }

        public override void Initialize(IClientCommandReader<TCommandInfo> commandReader, NameValueCollection settings)
        {
            base.Initialize(commandReader, settings);

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

        private void ExecuteCommand(TCommandInfo commandInfo)
        {
            
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

        public void StartReceive()
        {
            if (!Client.ReceiveAsync(m_ReceiveEventArgs))
                ProcessReceive(m_ReceiveEventArgs);
        }

        public void Send(byte[] data, int offset, int length)
        {
            try
            {
                Client.Send(data, offset, length, SocketFlags.None);
            }
            catch (Exception)
            {
                EnsureSocketClosed();
                OnClosed();
            }
        }
    }
}
