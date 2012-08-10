using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.RemoteProcessService.Command
{
    public class QUIT : StringCommandBase<RemoteProcessSession>
    {
        #region CommandBase<RemotePrcessSession> Members

        public override void ExecuteCommand(RemoteProcessSession session, StringRequestInfo requestInfo)
        {
            session.Send("bye");
            session.Close();
        }

        #endregion
    }
}
