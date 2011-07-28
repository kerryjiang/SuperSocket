using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.ClientEngine
{
    public class SocketSession<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        private Socket m_Socket;

        private SocketAsyncEventArgs m_ReceiveEventArgs;

        private byte[] m_ReceiveBuffer;

        private IClientCommandReader<TCommandInfo> m_CommandReader;

        public SocketSession(Socket socket, int receiveBufferSize, IClientCommandReader<TCommandInfo> commandReader)
        {
            m_Socket = socket;
            m_ReceiveEventArgs = new SocketAsyncEventArgs();
            m_ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_ReceiveEventArgs_Completed);
            m_ReceiveBuffer = new byte[receiveBufferSize];
            m_ReceiveEventArgs.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);
            m_CommandReader = commandReader;

            StartReceive();
        }

        void m_ReceiveEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
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

                var commandInfo = m_CommandReader.GetCommandInfo(e.Buffer, offset, length, out left);

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

        }

        public void StartReceive()
        {
            if (!m_Socket.ReceiveAsync(m_ReceiveEventArgs))
                ProcessReceive(m_ReceiveEventArgs);
        }

        public void Send(byte[] data, int offset, int length)
        {
            try
            {
                m_Socket.Send(data, offset, length, SocketFlags.None);
            }
            catch (Exception)
            {
                EnsureSocketClosed();
                OnClosed();
            }
        }

        private EventHandler m_Closed;

        public event EventHandler Closed
        {
            add { m_Closed += value; }
            remove { m_Closed -= value; }
        }

        private void OnClosed()
        {
            m_ReceiveBuffer = null;
            m_ReceiveEventArgs = null;

            var handler = m_Closed;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
