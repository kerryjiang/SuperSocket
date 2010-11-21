using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class QUIT : StringCommandBase<RemoteProcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        public override void ExecuteCommand(RemoteProcessSession session, StringCommandInfo commandData)
        {
            session.SendResponse("bye");
            session.Close();
        }

        #endregion
    }
}
