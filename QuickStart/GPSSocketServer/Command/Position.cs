using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.GPSSocketServer.Command
{
    public class Position : CommandBase<GPSSession, BufferedPackageInfo>
    {
        public override string Name
        {
            get
            {
                return "1A";
            }
        }

        public override void ExecuteCommand(GPSSession session, BufferedPackageInfo requestInfo)
        {
            //The logic of saving GPS position data
            var response = session.AppServer.DefaultResponse;
            session.Send(response, 0, response.Length); ;
        }
    }
}
