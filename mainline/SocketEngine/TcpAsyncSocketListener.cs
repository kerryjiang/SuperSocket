using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Tcp socket listener in async mode
    /// </summary>
    class TcpAsyncSocketListener : ISocketListener
    {
        public ListenerInfo Info { get; private set; }

        public IPEndPoint EndPoint
        {
            get { return Info.EndPoint; }
        }

        private int m_ListenBackLog;

        private Socket m_ListenSocket;

        public TcpAsyncSocketListener(ListenerInfo info)
        {
            Info = info;
            m_ListenBackLog = info.BackLog;
        }

        /// <summary>
        /// Starts to listen
        /// </summary>
        /// <returns></returns>
        public bool Start()
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
                EnsureClose();
                OnError(e);
                return false;
            }
        }

        void EnsureClose()
        {
            if (m_ListenSocket != null)
            {
                lock (this)
                {
                    if (m_ListenSocket != null)
                    {
                        try
                        {
                            m_ListenSocket.Close();
                        }
                        catch
                        {

                        }

                        m_ListenSocket = null;
                    }
                }
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
                if(e.SocketError != SocketError.OperationAborted)
                    OnError(e.SocketError.ToString());
                EnsureClose();
                return;
            }

            OnNewClientAccepted(e.AcceptSocket);

            e.AcceptSocket = null;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = m_ListenSocket.AcceptAsync(e);
            }
            catch (ObjectDisposedException)//listener has been stopped
            {
                EnsureClose();
                return;
            }
            catch (NullReferenceException)
            {
                EnsureClose();
                return;
            }
            catch (Exception exc)
            {
                EnsureClose();
                OnError(exc);
                return;
            }

            if (!willRaiseEvent)
                ProcessAccept(e);
        }

        public void Stop()
        {
            EnsureClose();
        }

        private NewClientAcceptHandler m_NewClientAccepted;

        /// <summary>
        /// Occurs when new client accepted.
        /// </summary>
        public event NewClientAcceptHandler NewClientAccepted
        {
            add { m_NewClientAccepted += value; }
            remove { m_NewClientAccepted -= value; }
        }

        private void OnNewClientAccepted(Socket socket)
        {
            m_NewClientAccepted.BeginInvoke(this, socket, null, null);
        }

        private ErrorHandler m_Error;

        /// <summary>
        /// Occurs when error got.
        /// </summary>
        public event ErrorHandler Error
        {
            add { m_Error += value; }
            remove { m_Error -= value; }
        }

        private void OnError(Exception e)
        {
            m_Error(this, e);
        }

        private void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
        }
    }
}
