using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.EchoService.Command
{
    public class ECHO : StringCommandBase<EchoSession>
    {
        #region CommandBase<EchoSession> Members

        public override void ExecuteCommand(EchoSession session, StringCommandInfo commandInfo)
        {
            session.SendResponse(commandInfo.Data);
        }

        #endregion
    }
}
