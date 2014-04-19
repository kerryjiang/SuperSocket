using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Pool;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketServer : TcpSocketServerBase, IActiveConnector
    {
        private int m_TotalConnections = 0;

        public AsyncSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {

        }

        protected override void OnNewClientAccepted(ISocketListener listener, Socket client, object state)
        {
            if (IsStopped)
                return;

            ProcessNewClient(client, listener.Info.Security);
        }

        private void CloseSessionForMaxConnectionReach(Socket client)
        {
            AppServer.AsyncRun(client.SafeClose);
            if (AppServer.Logger.IsErrorEnabled)
                AppServer.Logger.ErrorFormat("Max connection number {0} was reached!", AppServer.Config.MaxConnectionNumber);
        }

        private bool IncreaseConnections(Socket client)
        {
            while (true)
            {
                var totalConnections = m_TotalConnections;

                if (totalConnections >= AppServer.Config.MaxConnectionNumber)
                {
                    CloseSessionForMaxConnectionReach(client);
                    return false;//Max connections reach
                }

                var newTotal = totalConnections + 1;

                if (Interlocked.CompareExchange(ref m_TotalConnections, newTotal, totalConnections) == totalConnections)
                    return true;//Increase the total connections successfully
            }
        }

        private IAppSession ProcessNewClient(Socket client, SslProtocols security)
        {
            if (!IncreaseConnections(client))
                return null;

            ISocketSession socketSession;

            if (security == SslProtocols.None)
                socketSession = new AsyncSocketSession(client, SaePool);
            else
                socketSession = new AsyncStreamSocketSession(client, security, BufferStatePool);

            var session = CreateSession(client, socketSession);

            if (session == null)
            {
                AppServer.AsyncRun(client.SafeClose);
                return null;
            }

            socketSession.Closed += SessionClosed;

            var negotiateSession = socketSession as INegotiateSocketSession;

            if (negotiateSession == null)
            {
                if (RegisterSession(session))
                {
                    AppServer.AsyncRun(() => socketSession.Start());
                }

                return session;
            }

            negotiateSession.NegotiateCompleted += OnSocketSessionNegotiateCompleted;
            negotiateSession.Negotiate();

            return null;
        }

        private void OnSocketSessionNegotiateCompleted(object sender, EventArgs e)
        {
            var socketSession = sender as ISocketSession;
            var negotiateSession = socketSession as INegotiateSocketSession;

            if (!negotiateSession.Result)
            {
                socketSession.Close(CloseReason.SocketError);
                return;
            }

            if (RegisterSession(negotiateSession.AppSession))
            {
                AppServer.AsyncRun(() => socketSession.Start());
            }
        }

        private bool RegisterSession(IAppSession appSession)
        {
            if (AppServer.RegisterSession(appSession))
                return true;

            appSession.SocketSession.Close(CloseReason.InternalError);
            return false;
        }

        public override void ResetSessionSecurity(IAppSession session, SslProtocols security)
        {
            ISocketSession socketSession;

            if (security == SslProtocols.None)
                socketSession = new AsyncSocketSession(session.SocketSession.Client, SaePool, true);
            else
                socketSession = new AsyncStreamSocketSession(session.SocketSession.Client, security, BufferStatePool, true);

            socketSession.Initialize(session);
            socketSession.Start();
        }

        void SessionClosed(ISocketSession session, CloseReason reason)
        {
            Interlocked.Decrement(ref m_TotalConnections);
        }

        public override void Stop()
        {
            if (IsStopped)
                return;

            lock (SyncRoot)
            {
                if (IsStopped)
                    return;

                base.Stop();
            }
        }

        class ActiveConnectState
        {
            public TaskCompletionSource<ActiveConnectResult> TaskSource { get; private set; }

            public Socket Socket { get; private set; }

            public ActiveConnectState(TaskCompletionSource<ActiveConnectResult> taskSource, Socket socket)
            {
                TaskSource = taskSource;
                Socket = socket;
            }
        }

        Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint)
        {
            var taskSource = new TaskCompletionSource<ActiveConnectResult>();
            var socket = new Socket(targetEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(targetEndPoint, OnActiveConnectCallback, new ActiveConnectState(taskSource, socket));
            return taskSource.Task;
        }

        private void OnActiveConnectCallback(IAsyncResult result)
        {
            var connectState = result.AsyncState as ActiveConnectState;

            try
            {
                var socket = connectState.Socket;
                socket.EndConnect(result);

                var session = ProcessNewClient(socket, SslProtocols.None);

                if (session == null)
                    connectState.TaskSource.SetException(new Exception("Failed to create session for this socket."));
                else
                    connectState.TaskSource.SetResult(new ActiveConnectResult { Result = true, Session = session });
            }
            catch (Exception e)
            {
                connectState.TaskSource.SetException(e);
            }
        }
    }
}
