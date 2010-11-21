using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class KILL : StringCommandBase<RemoteProcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        public override void ExecuteCommand(RemoteProcessSession session, StringCommandInfo commandData)
        {
            int processId;

            string processKey = commandData.Parameters.FirstOrDefault();

            if (string.IsNullOrEmpty(processKey))
            {
                session.SendResponse("No parameter!");
                return;
            }

            if (int.TryParse(processKey, out processId))
            {
                Process process = Process.GetProcessById(processId);
                if (process != null)
                {
                    process.Kill();
                    session.SendResponse("The specific process has been killed!");
                    return;
                }
                else
                {
                    session.SendResponse("The specific process does not exist!");
                }
            }
            else
            {
                List<Process> processes = Process.GetProcesses().Where(p =>
                    p.ProcessName.Equals(processKey, StringComparison.OrdinalIgnoreCase)).ToList();

                processes.ForEach(p => p.Kill());

                if (processes.Count <= 0)
                    session.SendResponse("The specific process does not exist!");
                else if (processes.Count == 1)
                    session.SendResponse("The specific process has been killed!");
                else
                    session.SendResponse(string.Format("The {0} specific process has been killed!", processes.Count));
            }
        }

        #endregion
    }
}
