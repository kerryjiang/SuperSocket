using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.FtpService.Command
{
	class MDTM : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			string filename = commandData.Param;

			if (string.IsNullOrEmpty(filename))
			{
				session.SendParameterError();
				return;
			}

			DateTime mdfTime = session.FtpServiceProvider.GetModifyTime(session.FtpContext, filename);

            if(session.Context.Status == SocketContextStatus.Error)
            {
                session.SendResponse(session.Context.Message);             
            }
            else
            {
			    session.SendResponse(string.Format(Resource.FileOk_213, mdfTime.ToString("yyyyMMddhhmmss")));
            }			
		}

		#endregion
	}
}
