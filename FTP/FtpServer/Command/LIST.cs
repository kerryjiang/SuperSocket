using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Command;
using System.IO;
using SuperSocket.FtpService.Storage;

namespace SuperSocket.FtpService.Command
{
	class LIST : ICommand<FtpSession>
	{
		#region ICommand<SocketSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

            List<ListItem> list = session.FtpServiceProvider.GetList(session.FtpContext);

            if (session.Context.Status == SocketContextStatus.Error)
            {
                session.SendResponse(session.Context.Message);
                return;
            }

			if (session.DataConn.RunDataConnection(session))
			{
				session.SendResponse(Resource.DataConnectionAccepted_150);
               
				try
				{
					session.DataConn.SendResponse(session.FtpContext, list);
				}
				catch (Exception)
				{
					session.SendResponse(Resource.DataConnectionCannotOpen_420);
				}

				session.CloseDataConnection();
				session.SendResponse(Resource.DataTransferComplete_226);
			}
			else
			{
				session.SendResponse(Resource.DataConnectionCannotOpen_420);
			}
		}

		#endregion
	}
}
