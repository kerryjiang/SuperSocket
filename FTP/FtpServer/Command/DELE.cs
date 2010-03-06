using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.Common;

namespace SuperSocket.FtpService.Command
{
	class DELE : ICommand<FtpSession>
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

			if(session.FtpServiceProvider.DeleteFile(session.Context, filename))
            {
				session.SendResponse(Resource.DeleteOk_250);
            }			
			else
			{
                session.SendResponse(session.Context.Message);
			}					
		}

		#endregion
	}
}
