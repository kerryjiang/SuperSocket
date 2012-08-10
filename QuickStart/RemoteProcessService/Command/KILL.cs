using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class KILL : StringCommandBase<RemoteProcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        public override void ExecuteCommand(RemoteProcessSession session, StringRequestInfo requestInfo)
        {
            int processId;

            string processKey = requestInfo.Parameters.FirstOrDefault();

            if (string.IsNullOrEmpty(processKey))
            {
                session.Send("No parameter!");
                return;
            }

            if (int.TryParse(processKey, out processId))
            {
                Process process = Process.GetProcessById(processId);
                if (process != null)
                {
                    process.Kill();
                    session.Send("The specific process has been killed!");
                    return;
                }
                else
                {
                    session.Send("The specific process does not exist!");
                }
            }
            else
            {
                List<Process> processes = Process.GetProcesses().Where(p =>
                    p.ProcessName.Equals(processKey, StringComparison.OrdinalIgnoreCase)).ToList();

                processes.ForEach(p => p.Kill());

                if (processes.Count <= 0)
                    session.Send("The specific process does not exist!");
                else if (processes.Count == 1)
                    session.Send("The specific process has been killed!");
                else
                    session.Send(string.Format("The {0} specific process has been killed!", processes.Count));
            }
        }

        #endregion
    }
}
