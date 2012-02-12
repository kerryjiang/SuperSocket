using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket.ClientEngine
{
    public class SslStreamTcpSession : TcpClientSession
    {
        private SslStream m_SslStream;

        private byte[] m_ReceiveBuffer;

        public SslStreamTcpSession(EndPoint remoteEndPoint)
            : base(remoteEndPoint)
        {

        }

        protected override void SocketEventArgsCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessConnect(e);
        }

        protected override void StartReceive(SocketAsyncEventArgs e)
        {
            var stream = new NetworkStream(Client);
            var sslStream = new SslStream(stream, false, ValidateRemoteCertificate);
            sslStream.BeginAuthenticateAsClient(HostName, OnAuthenticated, sslStream);
        }

        private void OnAuthenticated(IAsyncResult result)
        {
            var sslStream = result.AsyncState as SslStream;

            try
            {
                sslStream.EndAuthenticateAsClient(result);
            }
            catch(Exception e)
            {
                OnError(e);
                return;
            }

            m_SslStream = sslStream;

            OnConnected();

            m_ReceiveBuffer = new byte[1024];
            m_SslStream.BeginRead(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, OnDataRead, m_SslStream);
        }

        private void OnDataRead(IAsyncResult result)
        {
            var sslStream = result.AsyncState as SslStream;
            int length = 0;

            try
            {
                length = sslStream.EndRead(result);
            }
            catch (Exception e) 
            {
                OnError(e);

                if(EnsureSocketClosed())
                    OnClosed();
            }

            OnDataReceived(m_ReceiveBuffer, 0, length);
            m_SslStream.BeginRead(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, OnDataRead, m_SslStream);
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                OnError(new Exception(sslPolicyErrors.ToString()));
                return false;
            }

            return true;
        }

        protected override void SendInternal(ArraySegment<byte> segment)
        {
            m_SslStream.BeginWrite(segment.Array, segment.Offset, segment.Count, OnWriteComplete, m_SslStream);
        }

        private void OnWriteComplete(IAsyncResult result)
        {
            var sslStream = result.AsyncState as SslStream;

            try
            {
                sslStream.EndWrite(result);
            }
            catch (Exception e)
            {
                IsSending = false;

                OnError(e);

                if (EnsureSocketClosed())
                    OnClosed();

                return;
            }

            DequeueSend();
        }
    }
}
