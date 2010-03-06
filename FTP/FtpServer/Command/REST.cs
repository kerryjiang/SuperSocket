using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	class REST : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			long offset = StringUtil.ParseLong(commandData.Param);

			session.Context.Offset = offset;

			session.SendResponse(Resource.RestartOk_350, offset);
		}

		#endregion
	}
}
