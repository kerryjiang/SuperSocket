using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    class UdpSocketServer<TPackageInfo> : SocketServerBase
        where TPackageInfo : IPackageInfo
    {
        private IPEndPoint m_EndPointIPv4;

        private IPEndPoint m_EndPointIPv6;

        private bool m_IsUdpRequestInfo = false;

        private IReceiveFilter<TPackageInfo> m_UdpRequestFilter;

        private int m_ConnectionCount = 0;

        private IRequestHandler<TPackageInfo> m_RequestHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpSocketServer&lt;TPackageInfo&gt;"/> class.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="listeners">The listeners.</param>
        public UdpSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {
            m_RequestHandler = appServer as IRequestHandler<TPackageInfo>;

            m_EndPointIPv4 = new IPEndPoint(IPAddress.Any, 0);
            m_EndPointIPv6 = new IPEndPoint(IPAddress.IPv6Any, 0);

            m_IsUdpRequestInfo = typeof(IUdpPackageInfo).IsAssignableFrom(typeof(TPackageInfo));

            m_UdpRequestFilter = ((IReceiveFilterFactory<TPackageInfo>)appServer.ReceiveFilterFactory).CreateFilter(appServer, null, null);
        }

        /// <summary>
        /// Called when [new client accepted].
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <param name="client">The client.</param>
        /// <param name="state">The state.</param>
        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            var eventArgs = state as SocketAsyncEventArgs;

            var remoteEndPoint = eventArgs.RemoteEndPoint as IPEndPoint;
            var receivedData = new ArraySegment<byte>(eventArgs.Buffer, eventArgs.Offset, eventArgs.BytesTransferred);

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
            finally
            {
                SaePool.Return(eventArgs.UserToken as SaeState);
            }
        }

        void ProcessPackageWithSessionID(Socket listenSocket, IPEndPoint remoteEndPoint, ArraySegment<byte> receivedData)
        {
            TPackageInfo requestInfo;

            string sessionID;

            int rest;

            try
            {
                var receiveData = new BufferList();
                receiveData.Add(receivedData);

                requestInfo = this.m_UdpRequestFilter.Filter(receiveData, out rest);
            }
            catch (Exception exc)
            {
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.Error("Failed to parse UDP package!", exc);
                return;
            }

            var udpRequestInfo = requestInfo as IUdpPackageInfo;

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
                if (!DetectConnectionNumber(remoteEndPoint))
                    return;

                var socketSession = new UdpSocketSession(listenSocket, remoteEndPoint, sessionID);
                appSession = AppServer.CreateAppSession(socketSession);

                if (appSession == null)
                    return;

                if (!DetectConnectionNumber(remoteEndPoint))
                    return;

                if (!AppServer.RegisterSession(appSession))
                    return;

                Interlocked.Increment(ref m_ConnectionCount);

                socketSession.Closed += OnSocketSessionClosed;
                socketSession.Start();
            }
            else
            {
                var socketSession = appSession.SocketSession as UdpSocketSession;
                //Client remote endpoint may change, so update session to ensure the server can find client correctly
                socketSession.UpdateRemoteEndPoint(remoteEndPoint);
            }

            m_RequestHandler.ExecuteCommand(appSession, requestInfo);
        }

        void ProcessPackageWithoutSessionID(Socket listenSocket, IPEndPoint remoteEndPoint, ArraySegment<byte> receivedData)
        {
            var sessionID = remoteEndPoint.ToString();
            var appSession = AppServer.GetSessionByID(sessionID);

            if (appSession == null) //New session
            {
                if (!DetectConnectionNumber(remoteEndPoint))
                    return;

                var socketSession = new UdpSocketSession(listenSocket, remoteEndPoint, sessionID);

                appSession = AppServer.CreateAppSession(socketSession);

                if (appSession == null)
                    return;

                if (!DetectConnectionNumber(remoteEndPoint))
                    return;

                if (!AppServer.RegisterSession(appSession))
                    return;

                Interlocked.Increment(ref m_ConnectionCount);
                socketSession.Closed += OnSocketSessionClosed;
                socketSession.Start();
            }

            ((UdpSocketSession)appSession.SocketSession).ProcessReceivedData(receivedData, null);
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
            return new UdpSocketListener(listenerInfo, SaePool);
        }

        public override void ResetSessionSecurity(IAppSession session, System.Security.Authentication.SslProtocols security)
        {
            throw new NotSupportedException();
        }
    }
}
