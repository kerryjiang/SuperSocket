using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;

namespace GiantSoft.FtpService.Command
{
	class PORT : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string address = commandData.GetFirstParam();

			string[] arrAddress = new string[0];

			if (!string.IsNullOrEmpty(address))
			{
				arrAddress = address.Split(',');
			}

			if (arrAddress == null || arrAddress.Length != 6)
			{
				session.SendParameterError();
				return;
			}

			string ip = arrAddress[0] + "." + arrAddress[1] + "." + arrAddress[2] + "." + arrAddress[3];
			int port = (Convert.ToInt32(arrAddress[4]) << 8) | Convert.ToInt32(arrAddress[5]);

			DataConnection dataConnection = new DataConnection(session, port);

			if (session.DataConn != null)
			{
				session.SendResponse(Resource.PortOk_220);
				return;
			}
			else
			{
				session.SendResponse(Resource.PortInvalid_552);
				return;
			}			
		}

		#endregion
	}
}
