using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	class SYST : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			session.SendResponse(Environment.OSVersion.ToString());
		}

		#endregion
    }
}
