﻿using System;
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
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Sockets;
using SuperSocket.SocketEngine.AsyncSocket;

namespace SuperSocket.SocketEngine
{
    class AsyncSocketServer : TcpSocketServerBase, IActiveConnector
    {
        public AsyncSocketServer(IAppServer appServer, ListenerInfo[] listeners)
            : base(appServer, listeners)
        {

        }

        private BufferManager m_BufferManager;

        private ConcurrentStack<SocketAsyncEventArgsProxy> m_ReadWritePool;

        public override bool Start()
        {
            try
            {
                int bufferSize = AppServer.Config.ReceiveBufferSize;

                if (bufferSize <= 0)
                    bufferSize = 1024 * 4;

                m_BufferManager = new BufferManager(bufferSize * AppServer.Config.MaxConnectionNumber, bufferSize);

                try
                {
                    m_BufferManager.InitBuffer();
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                    return false;
                }

                // preallocate pool of SocketAsyncEventArgs objects
                ISocketAsyncEventArgs socketEventArg;

                var socketArgsProxyList = new List<SocketAsyncEventArgsProxy>(AppServer.Config.MaxConnectionNumber);

                for (int i = 0; i < AppServer.Config.MaxConnectionNumber; i++)
                {
                    //Pre-allocate a set of reusable SocketAsyncEventArgs
                    socketEventArg = AppServer.SocketFactory.CreateSocketAsyncEventArgs();
                    var bufferInfo = m_BufferManager.SetBuffer();
                    if (bufferInfo.Item1)
                        socketEventArg.SetBuffer(m_BufferManager.Buffer, bufferInfo.Item2, m_BufferManager.BufferSize);

                    socketArgsProxyList.Add(new SocketAsyncEventArgsProxy(socketEventArg));
                }

                m_ReadWritePool = new ConcurrentStack<SocketAsyncEventArgsProxy>(socketArgsProxyList);

                if (!base.Start())
                    return false;

                IsRunning = true;
                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                return false;
            }
        }

        protected override void OnNewClientAccepted(ISocketListener listener, ISocket client, object state)
        {
            if (IsStopped)
                return;

            ProcessNewClient(client, listener.Info.Security);
        }

        private IAppSession ProcessNewClient(ISocket client, SslProtocols security)
        {
            //Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgsProxy socketEventArgsProxy;
            if (!m_ReadWritePool.TryPop(out socketEventArgsProxy))
            {
                AppServer.AsyncRun(client.SafeClose);
                if (AppServer.Logger.IsErrorEnabled)
                    AppServer.Logger.ErrorFormat("Max connection number {0} was reached!", AppServer.Config.MaxConnectionNumber);

                return null;
            }

            ISocketSession socketSession;

            if (security == SslProtocols.None)
                socketSession = new AsyncSocketSession(client, socketEventArgsProxy, this.AppServer.SocketFactory);
            else
                socketSession = new AsyncStreamSocketSession(client, security, socketEventArgsProxy);

            var session = CreateSession(client, socketSession);

            if (session == null)
            {
                socketEventArgsProxy.Reset();
                this.m_ReadWritePool.Push(socketEventArgsProxy);
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

            var socketAsyncProxy = ((IAsyncSocketSessionBase)session.SocketSession).SocketAsyncProxy;

            if (security == SslProtocols.None)
                socketSession = new AsyncSocketSession(session.SocketSession.Client, socketAsyncProxy, true, this.AppServer.SocketFactory);
            else
                socketSession = new AsyncStreamSocketSession(session.SocketSession.Client, security, socketAsyncProxy, true);

            socketSession.Initialize(session);
            socketSession.Start();
        }

        void SessionClosed(ISocketSession session, CloseReason reason)
        {
            var socketSession = session as IAsyncSocketSessionBase;
            if (socketSession == null)
                return;

            var proxy = socketSession.SocketAsyncProxy;
            proxy.Reset();
            var args = proxy.SocketEventArgs;

            var serverState = AppServer.State;
            var pool = this.m_ReadWritePool;

            if (pool == null || serverState == ServerState.Stopping || serverState == ServerState.NotStarted)
            {
                if(!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    args.Dispose();
                return;
            }

            if (proxy.OrigOffset != args.Offset)
            {
                args.SetBuffer(proxy.OrigOffset, AppServer.Config.ReceiveBufferSize);
            }

            if (!proxy.IsRecyclable)
            {
                //cannot be recycled, so release the resource and don't return it to the pool
                args.Dispose();
                return;
            }

            pool.Push(proxy);
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

                foreach (var item in m_ReadWritePool)
                    item.SocketEventArgs.Dispose();

                m_ReadWritePool = null;
                m_BufferManager = null;
                IsRunning = false;
            }
        }

        class ActiveConnectState
        {
            public TaskCompletionSource<ActiveConnectResult> TaskSource { get; private set; }

            public ISocket Socket { get; private set; }

            public ActiveConnectState(TaskCompletionSource<ActiveConnectResult> taskSource, ISocket socket)
            {
                TaskSource = taskSource;
                Socket = socket;
            }
        }

        Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint)
        {
            return ((IActiveConnector)this).ActiveConnect(targetEndPoint, null);
        }

        Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint, EndPoint localEndPoint)
        {
            var taskSource = new TaskCompletionSource<ActiveConnectResult>();
            var socket = this.AppServer.SocketFactory.Create(targetEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (localEndPoint != null)
            {
                socket.ExclusiveAddressUse = false;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.Bind(localEndPoint);
            }

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
