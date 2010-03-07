using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.FtpService.Storage;

namespace SuperSocket.FtpService.Command
{
	class RNFR : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string filepath = commandData.Param;

			if (string.IsNullOrEmpty(filepath))
			{
				session.SendParameterError();
				return;
			}

			long folderID = 0;

            if (session.FtpServiceProvider.IsExistFile(session.FtpContext, filepath))
			{
                session.FtpContext.RenameFor = filepath;
                session.FtpContext.RenameItemType = ItemType.File;
				session.SendResponse(Resource.RenameForOk_350);
			}
            else if (session.FtpServiceProvider.IsExistFolder(session.FtpContext, filepath, out folderID))
			{
                session.FtpContext.RenameFor = filepath;
                session.FtpContext.RenameItemType = ItemType.Folder;
                session.SendResponse(Resource.RenameForOk_350);
			}
			else
			{
				session.SendResponse(session.Context.Message);
			}
		}

		#endregion
	}
}
