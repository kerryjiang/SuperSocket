using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace RemoteProcessService.Command
{
    public class QUIT : ICommand<RemotePrcessSession>
    {
        #region ICommand<RemotePrcessSession> Members

        public void Execute(RemotePrcessSession session, CommandInfo commandData)
        {
            session.SendResponse("bye");
            session.Close();
        }

        #endregion
    }
}
