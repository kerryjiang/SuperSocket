using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.QuickStart.EchoService.Command
{
    public class ECHO : CommandBase<EchoSession>
    {
        #region CommandBase<EchoSession> Members

        protected override void Execute(EchoSession session, CommandInfo commandData)
        {
            session.SendResponse(commandData.Param);
        }

        #endregion
    }
}
