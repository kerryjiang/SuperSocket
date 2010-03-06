using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Security.Authentication;

namespace SuperSocket.FtpService.Command
{
	class PROT : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			if (session.SecureProtocol == SslProtocols.None)
			{
				session.SendResponse(Resource.ProtDisabled_431);
				return;
			}

			string level = commandData.Param;

			if (string.IsNullOrEmpty(level) || level.Length > 1)
			{
				session.SendParameterError();
				return;
			}

			switch (level[0])
			{
				case ('C'):
				case ('c'):
					session.Context.DataSecureProtocol = SslProtocols.None;
					break;
				case ('P'):
				case ('p'):
					session.Context.DataSecureProtocol = session.SecureProtocol;
					break;
				default:
					session.SendResponse(Resource.ProtectionLevelUnknow_504);
					return;
			}

			session.Context.ResetState();
			session.SendResponse(Resource.ProtOk_200);
		}

		#endregion
	}
}
