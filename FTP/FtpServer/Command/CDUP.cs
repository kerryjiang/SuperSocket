using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.FtpService.Command
{
	public class CDUP : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			if (string.IsNullOrEmpty(session.FtpContext.CurrentPath) || session.FtpContext.CurrentPath == "/")
			{
				session.SendResponse(Resource.NotFound_550);
				return;
			}

			string path = StringUtil.GetParentDirectory(session.FtpContext.CurrentPath, '/');

			long folderID;

			if (session.FtpServiceProvider.IsExistFolder(session.FtpContext, path, out folderID))
			{
				session.FtpContext.CurrentPath = path;

				if (folderID > 0)
					session.FtpContext.CurrentFolderID = folderID;

				session.SendResponse(string.Format(Resource.ChangeDirectoryUp_250, path));
			}
			else
			{
                if(session.Context.Status == SocketContextStatus.Error)
                    session.SendResponse(session.Context.Message);
                else
				    session.SendResponse(Resource.NotFound_550);
			}			
		}

		#endregion
	}
}
