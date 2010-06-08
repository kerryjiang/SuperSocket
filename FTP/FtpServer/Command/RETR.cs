using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	public class RETR : ICommand<FtpSession>
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

			try
			{
				if (session.DataConn.RunDataConnection(session))
				{
					session.SendResponse(Resource.DataConnectionAccepted_150);
                    if (session.FtpServiceProvider.ReadFile(session.FtpContext, filename, session.DataConn.GetStream(session.FtpContext)))
                        session.SendResponse(Resource.DataTransferComplete_226);
                    else
                        session.SendResponse(session.Context.Message);
				}
				else
				{
					session.SendResponse(Resource.DataConnectionCannotOpen_420);
				}
			}
			catch (SocketException)
			{
				session.SendResponse(Resource.DataConnectionError_426);
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				session.SendResponse(Resource.InputFileError_551, filename);
			}
			finally
			{
				session.CloseDataConnection();
			}
		}

		#endregion
	}
}
