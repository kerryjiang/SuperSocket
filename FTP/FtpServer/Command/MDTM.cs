using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;
using GiantSoft.Common;
using GiantSoft.SocketServiceCore;

namespace GiantSoft.FtpService.Command
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

			DateTime mdfTime = session.FtpServiceProvider.GetModifyTime(session.Context, filename);

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
