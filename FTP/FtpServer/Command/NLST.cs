using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;

namespace GiantSoft.FtpService.Command
{
	class NLST : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
            session.SendResponse(Resource.NotImplement_502);
		}

		#endregion
	}
}
