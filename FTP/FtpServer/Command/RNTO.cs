using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.Common;
using SuperSocket.FtpService.Storage;

namespace SuperSocket.FtpService.Command
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
