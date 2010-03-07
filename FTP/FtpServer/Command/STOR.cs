using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using SuperSocket.Common;
using SuperSocket.FtpService.Storage;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	class STOR : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public virtual void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string filename = commandData.Param;

			if (string.IsNullOrEmpty(filename))
			{
				session.SendParameterError();
				return;
			}

			if (session.DataConn.RunDataConnection(session))
			{
				Stream stream = session.DataConn.GetStream(session.Context);
								
				try
				{
					session.SendResponse(Resource.DataConnectionAccepted_150);

                    if (session.FtpServiceProvider.StoreFile(session.FtpContext, filename, stream))
                        session.SendResponse(Resource.DataTransferComplete_226);
                    else
                        session.SendResponse(session.Context.Message);
				}				
				catch (SocketException)
				{
					session.SendResponse(Resource.DataConnectionError_426);
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
					session.SendResponse(Resource.OuputFileError_551);
				}
				finally
				{
					session.CloseDataConnection();
				}
			}			
		}

		#endregion
	}
}
