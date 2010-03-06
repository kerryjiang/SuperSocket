using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	class ABOR : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			session.Context.ResetState();
			session.CloseDataConnection();
			session.SendResponse(Resource.AbortOk_226);
		}

		#endregion
	}
}
