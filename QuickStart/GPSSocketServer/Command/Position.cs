using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.GPSSocketServer.Command
{
    public class Position : CommandBase<GPSSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "1A";
            }
        }

        public override void ExecuteCommand(GPSSession session, BinaryRequestInfo requestInfo)
        {
            //The logic of saving GPS position data
            var response = session.AppServer.DefaultResponse;
            session.Send(response, 0, response.Length); ;
        }
    }
}
