using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	public class USER : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (session.Context.Logged)
			{
				session.SendResponse(Resource.AlreadyLoggedIn_230);
				return;
			}

			string username = commandData.Param;

			if (string.IsNullOrEmpty(username))
			{
				session.SendParameterError();
				return;
			}
			else if (string.Compare(username, "anonymous", StringComparison.OrdinalIgnoreCase) == 0)
			{
				session.SendResponse(Resource.RequirePasswor_331, username);
				session.Context.UserName = username;
				return;
			}
			else
			{
                session.SendResponse(Resource.RequirePasswor_331, username);
				session.Context.UserName = username;
				return;
			}
		}

		#endregion
	}
}
