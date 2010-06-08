using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.FtpService.Command
{
	public class SIZE : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string filename = commandData.Param;

			if (string.IsNullOrEmpty(filename))
			{
				session.SendParameterError();
				return;
			}

            long size = session.FtpServiceProvider.GetFileSize(session.FtpContext, filename);

            if (session.Context.Status == SocketContextStatus.Error)
                session.SendResponse(session.Context.Message);
            else
                session.SendResponse(Resource.SizeOk_213, size);
		}

		#endregion
	}
}
