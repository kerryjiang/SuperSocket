using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Diagnostics;

namespace RemoteProcessService.Command
{
    public class KILL : ICommand<RemotePrcessSession>
    {
        #region ICommand<RemotePrcessSession> Members

        public void Execute(RemotePrcessSession session, CommandInfo commandData)
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
                Process[] processes = Process.GetProcessesByName(processKey);
                processes.ToList().ForEach(p => p.Kill());
                if (processes.Length <= 0)
                    session.SendResponse("The specific process does not exist!");
                else if(processes.Length == 1)
                    session.SendResponse("The specific process has been killed!");
                else
                    session.SendResponse(string.Format("The {0} specific process has been killed!", processes.Length));
            }
        }

        #endregion
    }
}
