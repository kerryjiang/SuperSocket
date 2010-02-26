using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;
using GiantSoft.Common;
using GiantSoft.FtpService.Storage;

namespace GiantSoft.FtpService.Command
{
	class RNTO : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string newfileName = commandData.Param;

			if (string.IsNullOrEmpty(newfileName))
			{
				session.SendParameterError();
				return;
			}

            if (session.Context.RenameItemType == ItemType.File)
            {
				if (!session.FtpServiceProvider.RenameFile(session.Context, session.Context.RenameFor, newfileName))
                {
                    session.SendResponse(session.Context.Message);
                    return;
                }
            }
            else
            {
                if (!session.FtpServiceProvider.RenameFolder(session.Context, session.Context.RenameFor, newfileName))
                {
                    session.SendResponse(session.Context.Message);
                    return;
                }
            }

            session.SendResponse(Resource.RenameToOk_250);
		}

		#endregion
	}
}
