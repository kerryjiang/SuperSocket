using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	class PBSZ : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			session.FtpContext.ResetState();
			session.SendResponse(Resource.PBSZ_200);
		}

		#endregion
	}
}
