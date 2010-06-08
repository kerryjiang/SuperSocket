using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	public class CWD : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string path = commandData.Param;

			if (string.IsNullOrEmpty(path))
			{
				session.SendParameterError();
				return;
			}

			long folderID;

            if (session.FtpServiceProvider.IsExistFolder(session.FtpContext, path, out folderID))
            {
                session.FtpContext.CurrentPath = path;
                session.FtpContext.CurrentFolderID = folderID;
                session.SendResponse(Resource.ChangeWorkDirOk_250, path);
            }
            else
            {
                if (session.Context.Status == SocketContextStatus.Error)
                    session.SendResponse(session.Context.Message);
                else
                    session.SendResponse(Resource.NotFound_550);
            }
		}

		#endregion
	}
}
