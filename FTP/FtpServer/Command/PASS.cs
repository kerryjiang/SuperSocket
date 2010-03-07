using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.FtpService.Membership;

namespace SuperSocket.FtpService.Command
{
	class PASS : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (session.Context.Logged)
			{
				session.SendResponse(Resource.AlreadyLoggedIn_230);
				return;
			}

			string password = commandData.Param;

			if (string.IsNullOrEmpty(password))
			{
				session.SendParameterError();
				return;
			}

			FtpUser user = null;

			AuthenticationResult result = AuthenticationResult.Success;


			if (session.Context.IsAnonymous)
				user = new Anonymous();
			else
				result = session.FtpServiceProvider.Authenticate(session.Context.UserName, password, out user);

			if (result == AuthenticationResult.Success)
			{
				if (FtpOnlineUsers.Logon(session.FtpContext, user))
				{					
					session.SendResponse(Resource.LoggedIn_230);
					session.Context.Logged = true;
				}
				else
				{
					session.SendResponse(Resource.ReachedLoginLimit_421);
					session.CloseDataConnection();
				}
			}
			else
			{
				session.SendResponse(Resource.AuthenticationFailed_530);
			}			
		}

		#endregion
	}
}
