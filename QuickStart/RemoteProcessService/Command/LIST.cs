using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class LIST : StringCommandBase<RemoteProcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        public override void ExecuteCommand(RemoteProcessSession session, StringRequestInfo requestInfo)
        {
            Process[] processes;

            string firstParam = requestInfo.GetFirstParam();

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

            session.Send(sb.ToString());
        }

        #endregion
    }
}
