using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class UdpSocketServer<TRequestInfo> : SocketServerBase, IActiveConnector
        where TRequestInfo : IRequestInfo
    {
        private IPEndPoint m_EndPointIPv4;

        private IPEndPoint m_EndPointIPv6;

        private bool m_IsUdpRequestInfo = false;

        private IReceiveFilter<TRequestInfo> m_UdpRequestFilter;

        private int m_ConnectionCount = 0;

        private IRequestHandler<TRequestInfo> m_RequestHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpSocketServer&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="listeners">The listeners.</param>
        public UdpSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {
            m_RequestHandler = appServer as IRequestHandler<TRequestInfo>;

            m_EndPointIPv4 = new IPEndPoint(IPAddress.Any, 0);
            m_EndPointIPv6 = new IPEndPoint(IPAddress.IPv6Any, 0);

            m_IsUdpRequestInfo = typeof(TRequestInfo).IsSubclassOf(typeof(UdpRequestInfo));

            m_UdpRequestFilter = ((IReceiveFilterFactory<TRequestInfo>)appServer.ReceiveFilterFactory).CreateFilter(appServer, null, null);
        }

        /// <summary>
        /// Called when [new client accepted].
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <param name="client">The client.</param>
        /// <param name="state">The state.</param>
        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            var paramArray = state as object[];

            var receivedData = paramArray[0] as byte[];
            var socketAddress = paramArray[1] as SocketAddress;
            var remoteEndPoint = (socketAddress.Family == AddressFamily.InterNetworkV6 ? m_EndPointIPv6.Create(socketAddress) : m_EndPointIPv4.Create(socketAddress)) as IPEndPoint;

            try
            {
                if (m_IsUdpRequestInfo)
                {
                    ProcessPackageWithSessionID(client, remoteEndPoint, receivedData);
                }
                else
                {
                    ProcessPackageWithoutSessionID(client, remoteEndPoint, receivedData);
                }
            }
            catch (Exception e)
            {
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.Error("Process UDP package error!", e);
            }
        }

        IAppSession CreateNewSession(Socket listenSocket, IPEndPoint remoteEndPoint, string sessionID)
        {
            if (!DetectConnectionNumber(remoteEndPoint))
                return null;

            var socketSession = new UdpSocketSession(listenSocket, remoteEndPoint, sessionID);
            var appSession = AppServer.CreateAppSession(socketSession);

            if (appSession == null)
                return null;

            if (!DetectConnectionNumber(remoteEndPoint))
                return null;

            if (!AppServer.RegisterSession(appSession))
                return null;

            Interlocked.Increment(ref m_ConnectionCount);

            socketSession.Closed += OnSocketSessionClosed;
            socketSession.Start();

            return appSession;
        }


        void ProcessPackageWithSessionID(Socket listenSocket, IPEndPoint remoteEndPoint, byte[] receivedData)
        {
            TRequestInfo requestInfo;
            
            string sessionID;

            int rest;

            try
            {
                requestInfo = this.m_UdpRequestFilter.Filter(receivedData, 0, receivedData.Length, false, out rest);
            }
            catch (Exception exc)
            {
                if(AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.Error("Failed to parse UDP package!", exc);
                return;
            }

            var udpRequestInfo = requestInfo as UdpRequestInfo;

            if (rest > 0)
            {
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.Error("The output parameter rest must be zero in this case!");
                return;
            }

            if (udpRequestInfo == null)
            {
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.Error("Invalid UDP package format!");
                return;
            }

            if (string.IsNullOrEmpty(udpRequestInfo.SessionID))
            {
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.Error("Failed to get session key from UDP package!");
                return;
            }

            sessionID = udpRequestInfo.SessionID;

            var appSession = AppServer.GetSessionByID(sessionID);

            if (appSession == null)
            {
                appSession = CreateNewSession(listenSocket, remoteEndPoint, sessionID);

                //Failed to create a new session
                if (appSession == null)
                    return;
            }
            else
            {
                var socketSession = appSession.SocketSession as UdpSocketSession;
                //Client remote endpoint may change, so update session to ensure the server can find client correctly
                socketSession.UpdateRemoteEndPoint(remoteEndPoint);
            }

            m_RequestHandler.ExecuteCommand(appSession, requestInfo);
        }

        void ProcessPackageWithoutSessionID(Socket listenSocket, IPEndPoint remoteEndPoint, byte[] receivedData)
        {
            var sessionID = remoteEndPoint.ToString();
            var appSession = AppServer.GetSessionByID(sessionID);

            if (appSession == null) //New session
            {
                appSession = CreateNewSession(listenSocket, remoteEndPoint, sessionID);

                //Failed to create a new session
                if (appSession == null)
                    return;

                appSession.ProcessRequest(receivedData, 0, receivedData.Length, false);
            }
            else //Existing session
            {
                appSession.ProcessRequest(receivedData, 0, receivedData.Length, false);
            }
        }

        void OnSocketSessionClosed(ISocketSession socketSession, CloseReason closeReason)
        {
            Interlocked.Decrement(ref m_ConnectionCount);
        }

        bool DetectConnectionNumber(EndPoint remoteEndPoint)
        {
            if (m_ConnectionCount >= AppServer.Config.MaxConnectionNumber)
            {
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.ErrorFormat("Cannot accept a new UDP connection from {0}, the max connection number {1} has been exceed!",
                        remoteEndPoint.ToString(), AppServer.Config.MaxConnectionNumber);

                return false;
            }

            return true;
        }

        protected override ISocketListener CreateListener(ListenerInfo listenerInfo)
        {
            return new UdpSocketListener(listenerInfo);
        }

        public override void ResetSessionSecurity(IAppSession session, System.Security.Authentication.SslProtocols security)
        {
            throw new NotSupportedException();
        }

        Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint)
        {
            var taskSource = new TaskCompletionSource<ActiveConnectResult>();
            var socket = new Socket(targetEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            var session = CreateNewSession(socket, (IPEndPoint)targetEndPoint, targetEndPoint.ToString());

            if (session == null)
                taskSource.SetException(new Exception("Failed to create session for this socket."));
            else
                taskSource.SetResult(new ActiveConnectResult { Result = true, Session = session });

            return taskSource.Task;
        }
    }
}
