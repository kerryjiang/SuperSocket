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
            if (e.SocketError != SocketError.Success)
            {
                //if(e.SocketError != SocketError.OperationAborted)
                //    OnError(e.SocketError.ToString());
                //EnsureClose();
                OnError(new SocketException((int)e.SocketError));
                return;
            }

            OnNewClientAccepted(e.AcceptSocket, null);

            e.AcceptSocket = null;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = m_ListenSocket.AcceptAsync(e);
            }
            catch (Exception exc)
            {
                OnError(exc);
                return;
            }

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
