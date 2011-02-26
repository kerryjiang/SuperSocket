using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common;
using System.Net;

namespace SuperSocket.QuickStart.SocksServer.Command
{
    public class CONN : CommandBase<SocksSession, BinaryCommandInfo>
    {
        public override void ExecuteCommand(SocksSession session, BinaryCommandInfo commandInfo)
        {
            session.Context.CommandCode = (int)commandInfo.Data[1];

            int port = (int)commandInfo.Data[2] * 100 + (int)commandInfo.Data[3];
            IPAddress ipAddress = new IPAddress(commandInfo.Data.CloneRange(4, 4));

            session.Context.UserID = Encoding.ASCII.GetString(commandInfo.Data, 8, commandInfo.Data.Length - 10);

            byte[] response = new byte[8];

            try
            {
                session.ConnectTargetSocket(new IPEndPoint(ipAddress, port));
                response[1] = 0x5a;
            }
            catch (Exception e)
            {
                session.Logger.LogError(e);
                response[1] = 0x5b;
            }

            session.SendResponse(response);
        }
    }
}
