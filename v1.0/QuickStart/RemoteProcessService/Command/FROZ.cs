using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class FROZ : CommandBase<RemotePrcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        protected override void Execute(RemotePrcessSession session, CommandInfo commandData)
        {
            var server = session.AppServer;

            string firstParam = commandData.GetFirstParam();

            if (string.IsNullOrEmpty(firstParam))
            {
                session.SendResponse("Invalid parameter!");
                return;
            }

            var param = commandData[1];

            if ("list".Equals(firstParam, StringComparison.OrdinalIgnoreCase))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var p in server.GetFrozedProcesses())
                {
                    sb.AppendLine(p);
                }
                sb.AppendLine();

                session.SendResponse(sb.ToString());
                return;
            }
            else if ("add".Equals(firstParam, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(param))
                {
                    session.SendResponse("Invalid parameter!");
                    return;
                }

                server.AddFrozedProcess(param);
                session.SendResponse(string.Format("Frozed process {0} has been added!", param));
                return;
            }
            else if ("remove".Equals(firstParam, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(param))
                {
                    session.SendResponse("Invalid parameter!");
                    return;
                }

                server.RemoveFrozedProcess(param);
                session.SendResponse(string.Format("Frozed process {0} has been removed!", param));
                return;
            }
            else if ("clear".Equals(firstParam, StringComparison.OrdinalIgnoreCase))
            {
                server.ClearFrozedProcess();
                session.SendResponse("All frozed process have been removed!");
                return;
            }
            else if ("stop".Equals(firstParam, StringComparison.OrdinalIgnoreCase))
            {
                server.StopFroze();
                session.SendResponse("Frozing has been stopped!");
                return;
            }
            else if ("start".Equals(firstParam, StringComparison.OrdinalIgnoreCase))
            {
                server.StartFroze();
                session.SendResponse("Frozing has been started!");
                return;
            }
        }

        #endregion
    }
}
