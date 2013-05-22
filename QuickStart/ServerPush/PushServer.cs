using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.ServerPush
{
    public class PushServer : AppServer
    {
        private Timer m_PushTimer;

        private int m_Interval = 60 * 1000; //1 minute

        protected override void OnStarted()
        {
            m_PushTimer = new Timer(OnPushTimerCallback);
            m_PushTimer.Change(m_Interval, m_Interval);
            base.OnStarted();
        }

        private void OnPushTimerCallback(object state)
        {
            try
            {
                m_PushTimer.Change(Timeout.Infinite, Timeout.Infinite);
                PushToAllClients();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                m_PushTimer.Change(m_Interval, m_Interval);
            }
        }

        private void PushToAllClients()
        {
            var now = DateTime.Now.ToString();

            foreach (var session in this.GetAllSessions())
            {
                session.Send(now);
            }
        }

        protected override void OnStopped()
        {
            if (m_PushTimer != null)
            {
                m_PushTimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_PushTimer.Dispose();
                m_PushTimer = null;
            }

            base.OnStopped();
        }
    }
}
