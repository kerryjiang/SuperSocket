using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.GPSSocketServer.Command
{
    public class KeepAlive : CommandBase<GPSSession, BinaryCommandInfo>
    {
        public override string Name
        {
            get
            {
                return "10";
            }
        }

        public override void ExecuteCommand(GPSSession session, BinaryCommandInfo commandInfo)
        {
            //You can do nothing, after this command is executed, the LastActiveTime of this session will be updated
            session.SendResponse(session.AppServer.DefaultResponse);
        }
    }
}
