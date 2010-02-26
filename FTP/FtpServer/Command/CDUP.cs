using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;
using GiantSoft.Common;
using GiantSoft.SocketServiceCore;

namespace GiantSoft.FtpService.Command
{
	class CDUP : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			if (string.IsNullOrEmpty(session.Context.CurrentPath) || session.Context.CurrentPath == "/")
			{
				session.SendResponse(Resource.NotFound_550);
				return;
			}

			string path = StringUtil.GetParentDirectory(session.Context.CurrentPath, '/');

			long folderID;

			if (session.FtpServiceProvider.IsExistFolder(session.Context, path, out folderID))
			{
				session.Context.CurrentPath = path;

				if (folderID > 0)
					session.Context.CurrentFolderID = folderID;

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
