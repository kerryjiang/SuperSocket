using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System.Threading;
using System.Net.Sockets;
using SuperSocket.Common;

namespace SuperSocket.SocketEngine
{
    class AsyncStreamSocketServer<TAppSession, TCommandInfo> : TcpSocketServerBase<AsyncStreamSocketSession<TAppSession, TCommandInfo>, TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public AsyncStreamSocketServer(IAppServer<TAppSession> appServer, IPEndPoint localEndPoint, ICustomProtocol<TCommandInfo> protocol)
            : base(appServer, localEndPoint, protocol)
        {
            
        }

        private Semaphore m_MaxConnectionSemaphore;

        private Socket m_ListenSocket = null;

        private Thread m_ListenThread = null;

        private AutoResetEvent m_TcpClientConnected;

        public override bool Start()
        {
            try
            {
                if (!base.Start())
                    return false;

                m_TcpClientConnected = new AutoResetEvent(false);

                if (m_ListenSocket == null)
                {
                    m_ListenThread = new Thread(StartListen);
                    m_ListenThread.Start();
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

        private void StartListen()
        {
            m_ListenSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_ListenSocket.Bind(this.EndPoint);
                m_ListenSocket.Listen(this.AppServer.Config.ListenBacklog);

                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            }
            catch (Exception e)
            {
                AppServer.Logger.LogError(e);
                OnStartupFinished();
                return;
            }

            m_MaxConnectionSemaphore = new Semaphore(this.AppServer.Config.MaxConnectionNumber, this.AppServer.Config.MaxConnectionNumber);

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);

            IsRunning = true;

            OnStartupFinished();

            while (!IsStopped)
            {
                m_MaxConnectionSemaphore.WaitOne();

                if (IsStopped)
                    break;

                acceptEventArg.AcceptSocket = null;

                bool willRaiseEvent = true;

                try
                {
                    willRaiseEvent = m_ListenSocket.AcceptAsync(acceptEventArg);
                }
                catch (ObjectDisposedException)//listener has been stopped
                {
                    break;
                }
                catch (NullReferenceException)
                {
                    break;
                }
                catch (Exception e)
                {
                    AppServer.Logger.LogError("Failed to accept new tcp client in async server!", e);
                    break;
                }

                if (!willRaiseEvent)
                    AceptNewClient(acceptEventArg);

                m_TcpClientConnected.WaitOne();
            }

            IsRunning = false;
        }

        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            AceptNewClient(e);
        }

        void AceptNewClient(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                var client = e.AcceptSocket;
                m_TcpClientConnected.Set();

                var session = RegisterSession(client, new AsyncStreamSocketSession<TAppSession, TCommandInfo>(client, Protocol.CreateCommandReader(AppServer)));

                if (session != null)
                {
                    session.Closed += new EventHandler<SocketSessionClosedEventArgs>(session_Closed);
                    session.Start();
                }
                else
                {
                    Async.Run(() => client.SafeCloseClientSocket(AppServer.Logger));
                }
            }
            else
            {
                m_TcpClientConnected.Set();
            }
        }

        void session_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            m_MaxConnectionSemaphore.Release();
        }

        public override void Stop()
        {
            base.Stop();

            if (m_ListenSocket != null)
            {
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }

            VerifySocketServerRunning(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRunning)
                    Stop();

                m_TcpClientConnected.Close();
                m_MaxConnectionSemaphore.Close();
            }

            base.Dispose(disposing);
        }
    }
}
