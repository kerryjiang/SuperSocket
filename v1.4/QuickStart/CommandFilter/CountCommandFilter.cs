using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using System.Threading;

namespace SuperSocket.QuickStart.CommandFilter
{
    public class CountCommandFilter : CommandFilterAttribute
    {
        private long m_Total = 0;

        public override void OnCommandExecuting(IAppSession session, ICommand command)
        {

        }

        public override void OnCommandExecuted(IAppSession session, ICommand command)
        {
            Interlocked.Increment(ref m_Total);
        }

        public long Total
        {
            get { return m_Total; }
        }
    }
}
