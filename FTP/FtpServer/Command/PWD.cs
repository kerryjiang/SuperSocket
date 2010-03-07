using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	class PWD : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			session.SendResponse(Resource.CurrentDirectory_257, session.FtpContext.CurrentPath);
		}

		#endregion
	}
}
