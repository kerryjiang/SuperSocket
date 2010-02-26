using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore;
using GiantSoft.SocketServiceCore.Command;
using System.IO;
using GiantSoft.FtpService.Storage;

namespace GiantSoft.FtpService.Command
{
	class LIST : ICommand<FtpSession>
	{
		#region ICommand<SocketSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

            List<ListItem> list = session.FtpServiceProvider.GetList(session.Context);

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
					session.DataConn.SendResponse(session.Context, list);
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
