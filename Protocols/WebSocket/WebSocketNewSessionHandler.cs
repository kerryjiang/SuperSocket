using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;

namespace SuperSocket.WebSocket
{
    class WebSocketNewSessionHandler : INewSessionHandler
    {
        private ISessionRegister m_SessionRegister;

        private ConcurrentQueue<IAppSession> m_OpenHandshakePendingQueue = new ConcurrentQueue<IAppSession>();

        private int m_OpenHandshakeTimeOut;

        private int m_HandshakePendingQueueCheckingInterval;

        private Timer m_HandshakePendingQueueCheckingTimer;

        private ILog m_Log;

        public void Initialize(ISessionRegister sessionRegister)
        {
            m_SessionRegister = sessionRegister;

            var appServer = AppContext.CurrentServer;

            m_Log = appServer.Logger;

            var config = appServer.Config;

            if (!int.TryParse(config.Options.GetValue("handshakePendingQueueCheckingInterval"), out m_HandshakePendingQueueCheckingInterval))
            {
                m_HandshakePendingQueueCheckingInterval = 60;// 1 minute default
                m_Log.ErrorFormat("Invalid configuration value： handshakePendingQueueCheckingInterval");
            }

            if (!int.TryParse(config.Options.GetValue("openHandshakeTimeOut"), out m_OpenHandshakeTimeOut))
            {
                m_OpenHandshakeTimeOut = 120;// 2 minute default
                m_Log.ErrorFormat("Invalid configuration value： openHandshakeTimeOut");
            }
        }

        public void AcceptNewSession(IAppSession session)
        {
            m_OpenHandshakePendingQueue.Enqueue(session);
        }


        public void Start()
        {
            m_HandshakePendingQueueCheckingTimer = new Timer(HandshakePendingQueueCheckingCallback, null, m_HandshakePendingQueueCheckingInterval * 1000, m_HandshakePendingQueueCheckingInterval * 1000);
        }

        private void HandshakePendingQueueCheckingCallback(object state)
        {
            try
            {
                m_HandshakePendingQueueCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);

                while (true)
                {
                    IAppSession session;

                    if (!m_OpenHandshakePendingQueue.TryPeek(out session))
                        break;

                    WebSocketContext context = WebSocketContext.Get(session);

                    if (!session.Connected || context.Handshaked)
                    {
                        //Handshaked or not connected
                        m_OpenHandshakePendingQueue.TryDequeue(out session);
                        continue;
                    }

                    if (DateTime.Now < session.StartTime.AddSeconds(m_OpenHandshakeTimeOut))
                        break;

                    //Timeout, dequeue and then close
                    m_OpenHandshakePendingQueue.TryDequeue(out session);
                    session.Close(CloseReason.TimeOut);
                }
            }
            catch (Exception e)
            {
                if (m_Log.IsErrorEnabled)
                    m_Log.Error(e);
            }
            finally
            {
                m_HandshakePendingQueueCheckingTimer.Change(m_HandshakePendingQueueCheckingInterval * 1000, m_HandshakePendingQueueCheckingInterval * 1000);
            }
        }
        public void Stop()
        {
            m_HandshakePendingQueueCheckingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
