using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.Facility.PolicyServer
{
    class PolicySession : IPolicySession
    {
        private Socket m_Client;
        private IPolicyServer m_Server;
        private byte[] m_Buffer;
        private int m_CurrentReceived = 0;

        public void Initialize(IPolicyServer server, Socket client, int expectedReceiveLength)
        {
            m_Client = client;
            m_Server = server;
            m_Buffer = new byte[expectedReceiveLength];
        }

        public void StartReceive(SocketAsyncEventArgs e)
        {
            if (!m_Client.ReceiveAsync(e))
            {
                ProcessReceive(e);
            }
        }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (m_CurrentReceived + e.BytesTransferred <= m_Buffer.Length)
            {
                Array.Copy(e.Buffer, e.Offset, m_Buffer, m_CurrentReceived, e.BytesTransferred);
                m_CurrentReceived += e.BytesTransferred;

                if (m_CurrentReceived < m_Buffer.Length)
                {
                    if (!m_Client.ReceiveAsync(e))
                        ProcessReceive(e);
                    return;
                }
            }
            else
            {
                Array.Copy(e.Buffer, e.Offset, m_Buffer, m_CurrentReceived, m_Buffer.Length - m_CurrentReceived);
            }

            m_Server.ValidateSession(this, e, m_Buffer);
        }

        public void SendResponse(byte[] response)
        {
            m_Client.Send(response);
        }

        public void Close()
        {
            try
            {
                m_Client.Shutdown(SocketShutdown.Both);
                m_Client.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
