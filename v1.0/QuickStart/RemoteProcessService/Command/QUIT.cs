using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class QUIT : CommandBase<RemotePrcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        protected override void Execute(RemotePrcessSession session, CommandInfo commandData)
        {
            session.SendResponse("bye");
            session.Close();
        }

        #endregion
    }
}
