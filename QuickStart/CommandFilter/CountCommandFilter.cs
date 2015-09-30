using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.QuickStart.CommandFilter
{
    public class CountCommandFilter : CommandFilterAttribute
    {
        private long m_Total = 0;

        public override void OnCommandExecuting(ICommandExecutingContext commandContext)
        {

        }

        public override void OnCommandExecuted(ICommandExecutingContext commandContext)
        {
            Interlocked.Increment(ref m_Total);
        }

        public long Total
        {
            get { return m_Total; }
        }
    }
}
