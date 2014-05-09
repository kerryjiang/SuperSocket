using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Tcp socket listener in async mode
    /// </summary>
    class TcpAsyncSocketListener : SocketListenerBase
    {
        private int m_ListenBackLog;

        private Socket m_ListenSocket;

        private SocketAsyncEventArgs m_AcceptSAE;

        public TcpAsyncSocketListener(ListenerInfo info)
            : base(info)
        {
            m_ListenBackLog = info.BackLog;
        }

        /// <summary>
        /// Starts to listen
        /// </summary>
        /// <param name="config">The server config.</param>
        /// <returns></returns>
        public override bool Start(IServerConfig config)
        {
            m_ListenSocket = new Socket(this.Info.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_ListenSocket.Bind(this.Info.EndPoint);
                m_ListenSocket.Listen(m_ListenBackLog);

                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
                m_AcceptSAE = acceptEventArg;
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);

                if (!m_ListenSocket.AcceptAsync(acceptEventArg))
                    ProcessAccept(acceptEventArg);

                return true;

            }
            catch (Exception e)
            {
                OnError(e);
                return false;
            }
        }


        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket socket = null;

            if (e.SocketError != SocketError.Success)
            {
                var errorCode = (int)e.SocketError;

                //The listen socket was closed
                if (errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                    return;

                OnError(new SocketException(errorCode));
            }
            else
            {
                socket = e.AcceptSocket;
            }

            e.AcceptSocket = null;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = m_ListenSocket.AcceptAsync(e);
            }
            catch (ObjectDisposedException)
            {
                //The listener was stopped
                //Do nothing
                //make sure ProcessAccept won't be executed in this thread
                willRaiseEvent = true;
            }
            catch (NullReferenceException)
            {
                //The listener was stopped
                //Do nothing
                //make sure ProcessAccept won't be executed in this thread
                willRaiseEvent = true;
            }
            catch (Exception exc)
            {
                OnError(exc);
                //make sure ProcessAccept won't be executed in this thread
                willRaiseEvent = true;
            }

            if (socket != null)
                OnNewClientAccepted(socket, null);

            if (!willRaiseEvent)
                ProcessAccept(e);
        }

        public override void Stop()
        {
            if (m_ListenSocket == null)
                return;

            lock (this)
            {
                if (m_ListenSocket == null)
                    return;

                m_AcceptSAE.Completed -= new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);
                m_AcceptSAE.Dispose();
                m_AcceptSAE = null;

                try
                {
                    m_ListenSocket.Close();
                }
                finally
                {
                    m_ListenSocket = null;
                }
            }

            OnStopped();
        }
    }
}
