using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.GPSSocketServer.Command
{
    public class KeepAlive : CommandBase<GPSSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "10";
            }
        }

        public override void ExecuteCommand(GPSSession session, BinaryRequestInfo requestInfo)
        {
            //You can do nothing, after this command is executed, the LastActiveTime of this session will be updated
            var response = session.AppServer.DefaultResponse;
            session.Send(response, 0, response.Length);
        }
    }
}
