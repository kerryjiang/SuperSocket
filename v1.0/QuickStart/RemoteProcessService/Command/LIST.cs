using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Diagnostics;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class LIST : CommandBase<RemotePrcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        protected override void Execute(RemotePrcessSession session, CommandInfo commandData)
        {
            Process[] processes;

            string firstParam = commandData.GetFirstParam();

            if (string.IsNullOrEmpty(firstParam) || firstParam == "*")
                processes = Process.GetProcesses();
            else
                processes = Process.GetProcesses().Where(p =>
                    p.ProcessName.IndexOf(firstParam, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

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
