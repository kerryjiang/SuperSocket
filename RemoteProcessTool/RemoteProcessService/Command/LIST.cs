using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Diagnostics;

namespace RemoteProcessService.Command
{
    public class LIST : ICommand<RemotePrcessSession>
    {
        #region ICommand<RemotePrcessSession> Members

        public void Execute(RemotePrcessSession session, CommandInfo commandData)
        {
            Process[] processes = Process.GetProcesses();

            StringBuilder sb = new StringBuilder();

            foreach (var p in processes)
            {
                sb.AppendLine(string.Format("{0}\t{1}", p.ProcessName, p.Id));
            }

            sb.AppendLine();

            session.SendResponse(sb.ToString());
        }

        #endregion
    }
}
