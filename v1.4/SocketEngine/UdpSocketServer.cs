using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class UdpSocketServer<TAppSession, TCommandInfo> : SocketServerBase<UdpSocketSession<TAppSession, TCommandInfo>, TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        private AutoResetEvent m_UdpClientConnected;

        private BufferManager m_BufferManager;

        private SocketAsyncEventArgs m_SocketAsyncEventArgs;

        private int m_LiveConnectionCount = 0;

        private const string m_InvalidEndPoint = "0.0.0.0:0";

        private bool m_SessionKeyFromCommandInfo = false;

        private ICommandReader<TCommandInfo> m_UdpCommandInfoReader;

        public UdpSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ICustomProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint, protocol)
        {
            if (typeof(TCommandInfo).IsSubclassOf(typeof(UdpCommandInfo)))
            {
                m_SessionKeyFromCommandInfo = true;
                m_UdpCommandInfoReader = Protocol.CreateCommandReader(this.AppServer);
            }
        }

        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

                m_UdpClientConnected = new AutoResetEvent(false);

                int bufferSize = Math.Max(AppServer.Config.ReceiveBufferSize, AppServer.Config.SendBufferSize);

                if (bufferSize <= 0)
                    bufferSize = 1024 * 8;

                //m_BufferManager = new BufferManager(bufferSize * AppServer.Config.MaxConnectionNumber * 2, bufferSize);
                m_BufferManager = new BufferManager(bufferSize * 2, bufferSize);

                try
                {
                    m_BufferManager.InitBuffer();
                }
                catch (Exception e)
                {
                    AppServer.Logger.LogError("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                    return false;
                }

                //Pre-allocate a set of reusable SocketAsyncEventArgs
                m_SocketAsyncEventArgs = new SocketAsyncEventArgs();
                m_SocketAsyncEventArgs.UserToken = new AsyncUserToken();
                m_SocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketEventArg_Completed);
                m_BufferManager.SetBuffer(m_SocketAsyncEventArgs);

                if (m_ListenSocket == null)
                {
                    m_ListenThread = new Thread(StartListen);
                    m_ListenThread.Start(m_SocketAsyncEventArgs);
                }

                WaitForStartupFinished();
                return IsRunning;
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                return false;
            }
        }

        void socketEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                OnSocketReceived(e);
            }
        }

        private void StartListen(object state)
        {
            var socketAsyncEventArgs = state as SocketAsyncEventArgs;
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                m_ListenSocket.Bind(this.EndPoint);
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                OnStartupFinished();
                return;
            }

            IsRunning = true;

            OnStartupFinished();

            bool willRaiseEvent = true;

            while (!IsStopped)
            {
                try
                {                    
                    socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    willRaiseEvent = m_ListenSocket.ReceiveFromAsync(socketAsyncEventArgs);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (Exception e)
                {
                    SocketException se = e as SocketException;
                    if (se != null)
                    {
                        //A blocking operation was interrupted by a call to WSACancelBlockingCall
                        //SocketListener has been stopped normally
                        if (se.ErrorCode == 10004 || se.ErrorCode == 10038)
                            break;
                    }

                    AppServer.Logger.LogError("Socket Listener stopped unexpectly, Socket Address:" + EndPoint.Address.ToString() + ":" + EndPoint.Port, e);
                    break;
                }

                if (!willRaiseEvent)
                    Async.Run(() => OnSocketReceived(socketAsyncEventArgs), (x) => AppServer.Logger.LogError(x));

                m_UdpClientConnected.WaitOne();
            }

            IsRunning = false;
        }

        protected UdpSocketSession<TAppSession, TCommandInfo> RegisterSession(UdpSocketSession<TAppSession, TCommandInfo> socketSession)
        {
            TAppSession appSession = this.AppServer.CreateAppSession(socketSession);

            if (appSession == null)
                return null;

            socketSession.Initialize(this.AppServer, appSession);
            return socketSession;
        }

        private void ProcessReceivedData(IPEndPoint remoteEndPoint, byte[] receivedData)
        {
            TAppSession appSession = AppServer.GetAppSessionByIndentityKey(remoteEndPoint.ToString());

            if (appSession == null) //New session
            {
                if (m_LiveConnectionCount >= AppServer.Config.MaxConnectionNumber)
                {
                    AppServer.Logger.LogError(string.Format("Cannot accept a new UDP connection from {0}, the max connection number {1} has been exceed!",
                        remoteEndPoint.ToString(), AppServer.Config.MaxConnectionNumber));
                    return;
                }

                var session = RegisterSession(new UdpSocketSession<TAppSession, TCommandInfo>(m_ListenSocket, remoteEndPoint, Protocol.CreateCommandReader(AppServer)));

                if (session == null)
                    return;

                Interlocked.Increment(ref m_LiveConnectionCount);
                session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
                session.Start();
                Async.Run(() => session.ProcessData(receivedData), (x) => AppServer.Logger.LogError(x));
            }
            else //Existing session
            {
                var session = appSession.SocketSession as UdpSocketSession<TAppSession, TCommandInfo>;
                Async.Run(() => session.ProcessData(receivedData), (x) => AppServer.Logger.LogError(x));
            }
        }

        private void ProcessReceivedDataWithSessionKey(IPEndPoint remoteEndPoint, byte[] receivedData)
        {
            TCommandInfo commandInfo;
            string sessionKey;

            try
            {
                int left;
                commandInfo = m_UdpCommandInfoReader.FindCommandInfo(null, receivedData, 0, receivedData.Length, false, out left);

                var udpCommandInfo = commandInfo as UdpCommandInfo;

                if (left > 0)
                {
                    AppServer.Logger.LogError("The output parameter left must be zero in this case!");
                    return;
                }

                if (udpCommandInfo == null)
                {
                    AppServer.Logger.LogError("Invalid UDP package format!");
                    return;
                }

                if (string.IsNullOrEmpty(udpCommandInfo.SessionKey))
                {
                    AppServer.Logger.LogError("Failed to get session key from UDP package!");
                    return;
                }

                sessionKey = udpCommandInfo.SessionKey;
            }
            catch (Exception exc)
            {
                AppServer.Logger.LogError("Failed to parse UDP package!", exc);
                return;
            }

            TAppSession appSession = AppServer.GetAppSessionByIndentityKey(sessionKey);

            if (appSession == null)
            {
                var socketSession = RegisterSession(new UdpSocketSession<TAppSession, TCommandInfo>(m_ListenSocket, remoteEndPoint, sessionKey));
                if (socketSession == null)
                    return;

                appSession = socketSession.AppSession;

                Interlocked.Increment(ref m_LiveConnectionCount);
                socketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
                socketSession.Start();
            }
            else
            {
                var socketSession = appSession.SocketSession as UdpSocketSession<TAppSession, TCommandInfo>;
                //Client remote endpoint may change, so update session to ensure the server can find client correctly
                socketSession.UpdateRemoteEndPoint(remoteEndPoint);
            }

            Async.Run(() => appSession.ExecuteCommand(appSession, commandInfo));
        }

        private void OnSocketReceived(SocketAsyncEventArgs e)
        {
            var receivedData = e.Buffer.CloneRange(e.Offset, e.BytesTransferred);
            var address = e.RemoteEndPoint.Serialize();

            m_UdpClientConnected.Set();

            var ipAddress = EndPoint.Create(address) as IPEndPoint;

            if (m_InvalidEndPoint.Equals(ipAddress.ToString()))
                return;

            if (m_SessionKeyFromCommandInfo)
            {
                ProcessReceivedDataWithSessionKey(ipAddress, receivedData);
            }
            else
            {
                ProcessReceivedData(ipAddress, receivedData);
            }
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            Interlocked.Decrement(ref m_LiveConnectionCount);
        }

        public override void Stop()
        {
            base.Stop();

            if (m_ListenSocket != null)
            {
                try
                {
                    m_ListenSocket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }

            if (m_BufferManager != null)
                m_BufferManager = null;

            m_SocketAsyncEventArgs = null;

            m_UdpClientConnected.Set();

            VerifySocketServerRunning(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();

                m_UdpClientConnected.Close();
            }

            base.Dispose(disposing);
        }
    }
}
