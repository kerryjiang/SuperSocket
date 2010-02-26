using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;

namespace GiantSoft.FtpService.Command
{
	class PWD : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			session.SendResponse(Resource.CurrentDirectory_257, session.Context.CurrentPath);
		}

		#endregion
	}
}
