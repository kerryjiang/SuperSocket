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
                ProcessConnect(e);
                return;
            }

            ProcessReceive(e);
        }

        protected override void Connect()
        {

#if SILVERLIGHT
            m_SocketEventArgs.RemoteEndPoint = RemoteEndPoint;

            if (!Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, m_SocketEventArgs))
                ProcessConnect(m_SocketEventArgs);
#else
            if (RemoteEndPoint is IPEndPoint)
            {
                m_SocketEventArgs.RemoteEndPoint = RemoteEndPoint;

                var ipEndPoint = RemoteEndPoint as IPEndPoint;
                var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_SocketEventArgs.UserToken = new ConnectStateToken
                    {
                        Addresses = new IPAddress[] { ipEndPoint.Address },
                        Port = ipEndPoint.Port,
                        CurrentConnectIndex = 0,
                        Socket = socket
                    };

                if (!socket.ConnectAsync(m_SocketEventArgs))
                    ProcessConnect(m_SocketEventArgs);
            }
            else if (RemoteEndPoint is DnsEndPoint)
            {
                var dnsEndPoint = RemoteEndPoint as DnsEndPoint;
                Dns.BeginGetHostAddresses(dnsEndPoint.Host, OnGetHostAddresses,
                    new ConnectStateToken
                    {
                        Port = dnsEndPoint.Port
                    });
            }
#endif
        }

#if !SILVERLIGHT
        private void OnGetHostAddresses(IAsyncResult state)
        {
            IPAddress[] addresses = Dns.EndGetHostAddresses(state);

            var connectState = state.AsyncState as ConnectStateToken;

            if(!Socket.OSSupportsIPv6)
                addresses = addresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToArray();

            if (addresses.Length <= 0)
                return;

            connectState.Addresses = addresses;

            var ipEndPoint = new IPEndPoint(addresses[0], connectState.Port);
            m_SocketEventArgs.RemoteEndPoint = ipEndPoint;

            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            connectState.Socket = socket;

            m_SocketEventArgs.UserToken = connectState;

            if (!socket.ConnectAsync(m_SocketEventArgs))
                ProcessConnect(m_SocketEventArgs);
        }
#endif

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
#if SILVERLIGHT
            if (e.SocketError != SocketError.Success)
            {
                OnError(new SocketException((int)e.SocketError));
                return;
            }

            Client = e.ConnectSocket;
#else
            var connectState = e.UserToken as ConnectStateToken;

            if (e.SocketError != SocketError.Success)
            {
                if (e.SocketError != SocketError.HostUnreachable)
                {
                    OnError(new SocketException((int)e.SocketError));
                    return;
                }

                if (connectState.Addresses.Length <= (connectState.CurrentConnectIndex + 1))
                {
                    OnError(new SocketException((int)SocketError.HostUnreachable));
                    return;
                }

                var currentConnectIndex = connectState.CurrentConnectIndex + 1;
                var currentIpAddress = connectState.Addresses[currentConnectIndex];

                e.RemoteEndPoint = new IPEndPoint(currentIpAddress, connectState.Port);
                connectState.CurrentConnectIndex = currentConnectIndex;

                var socket = new Socket(currentIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                connectState.Socket = socket;

                if (!socket.ConnectAsync(m_SocketEventArgs))
                    ProcessConnect(m_SocketEventArgs);

                return;
            }

            Client = connectState.Socket;
            e.UserToken = null;
#endif
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
                ProcessConnect(e);
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
                OnError(new SocketException((int)e.SocketError));
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
            catch(Exception e)
            {
                EnsureSocketClosed();
                OnError(e);
                return;
            }

            if (!async)
                ProcessReceive(m_SocketEventArgs);
        }

        private void DetectConnected()
        {
            if (Client != null)
                return;

            throw new Exception("The socket is not connected!", new SocketException((int)SocketError.NotConnected));
        }

        private ConcurrentQueue<ArraySegment<byte>> m_SendingQueue = new ConcurrentQueue<ArraySegment<byte>>();

        private volatile bool m_IsSending = false;

        public override void Send(byte[] data, int offset, int length)
        {
            DetectConnected();

            m_SendingQueue.Enqueue(new ArraySegment<byte>(data, offset, length));

            if (!m_IsSending)
            {
                DequeueSend();
            }
        }

        public override void Send(IList<ArraySegment<byte>> segments)
        {
            DetectConnected();

            for (var i = 0; i < segments.Count; i++)
                m_SendingQueue.Enqueue(segments[i]);

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
            catch(Exception e)
            {
                EnsureSocketClosed();
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
                m_IsSending = false;
                return;
            }

            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                m_IsSending = false;
                EnsureSocketClosed();
                OnClosed();

                if (e.SocketError != SocketError.Success)
                    OnError(new SocketException((int)e.SocketError));

                return;
            }

            DequeueSend();
        }
    }
}
