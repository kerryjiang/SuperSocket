using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.CommandFilter
{
    public class LogTimeCommandFilter : CommandFilterAttribute
    {
        public override void OnCommandExecuting(IAppSession session, ICommand command)
        {
            session.Items["StartTime"] = DateTime.Now;
        }

        public override void OnCommandExecuted(IAppSession session, ICommand command)
        {
            var startTime = session.Items.GetValue<DateTime>("StartTime");
            var ts = DateTime.Now.Subtract(startTime);

            if (ts.TotalSeconds > 5)
            {
                session.Logger.LogPerf(string.Format("A command '{0}' took {1} seconds!", command.Name, ts.ToString()));
            }
        }
    }
}
