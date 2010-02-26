using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;

namespace GiantSoft.FtpService.Command
{
	class QUIT : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			session.CloseDataConnection();
			session.SendResponse(Resource.GoodBye_221);
			session.Close();
		}

		#endregion
	}
}
