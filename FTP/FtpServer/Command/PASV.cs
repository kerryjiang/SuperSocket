using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Net;

namespace SuperSocket.FtpService.Command
{
	class PASV : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			DataConnection dataConnection = new DataConnection(session);

			if (dataConnection.Port > 0)
			{
				string address = ((IPEndPoint)session.LocalEndPoint).Address.ToString().Replace('.', ',') + "," + (dataConnection.Port >> 8) + "," + (dataConnection.Port & 0xFF);
				session.SendResponse(Resource.PassiveEnter_227, address);
			}
			else
			{
				session.SendResponse(Resource.DataConnectionCannotOpen_420);
			}
		}

		#endregion
	}
}
