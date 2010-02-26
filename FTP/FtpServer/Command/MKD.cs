using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;
using GiantSoft.Common;

namespace GiantSoft.FtpService.Command
{
	class MKD : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string foldername = commandData.Param;

			if (string.IsNullOrEmpty(foldername))
			{
				session.SendParameterError();
				return;
			}

			if(session.FtpServiceProvider.CreateFolder(session.Context, foldername))
            {
				session.SendResponse(Resource.MakeDirOk_250, session.Context.CurrentPath + "/" + foldername);
			}
			else
			{
                session.SendResponse(session.Context.Message);
			}
		}

		#endregion
	}
}
