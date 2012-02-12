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
            try
            {
                var sslStream = new SslStream(new NetworkStream(Client), false, ValidateRemoteCertificate);
                sslStream.BeginAuthenticateAsClient(HostName, OnAuthenticated, sslStream);
            }
            catch (Exception exc)
            {
                OnError(exc);

                if (EnsureSocketClosed())
                    OnClosed();
            }
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
            BeginRead();
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

                return;
            }

            if (length == 0)
            {
                if (EnsureSocketClosed())
                    OnClosed();

                return;
            }

            OnDataReceived(m_ReceiveBuffer, 0, length);
            BeginRead();
        }

        void BeginRead()
        {
            try
            {
                m_SslStream.BeginRead(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, OnDataRead, m_SslStream);
            }
            catch (Exception e)
            {
                OnError(e);

                if (EnsureSocketClosed())
                    OnClosed();
            }
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
            try
            {
                m_SslStream.BeginWrite(segment.Array, segment.Offset, segment.Count, OnWriteComplete, m_SslStream);
            }
            catch (Exception e)
            {
                OnError(e);

                if (EnsureSocketClosed())
                    OnClosed();
            }
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

            if (!DequeueSend())
            {
                try
                {
                    m_SslStream.Flush();
                }
                catch (Exception e)
                {
                    OnError(e);

                    if (EnsureSocketClosed())
                        OnClosed();
                }
            }
        }
    }
}
