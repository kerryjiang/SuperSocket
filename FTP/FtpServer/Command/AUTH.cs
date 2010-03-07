using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;
using System.Security.Authentication;

namespace SuperSocket.FtpService.Command
{
	class AUTH : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			if (!session.Context.Logged)
				return;

			string ssl = commandData.Param;

			switch (ssl)
			{
				case ("SSL"):
					session.SecureProtocol = SslProtocols.Ssl3;
					break;
				case ("SSL2"):
                    session.SecureProtocol = SslProtocols.Ssl2;
					break;
				case ("TLS"):
                    session.SecureProtocol = SslProtocols.Tls;
					break;
				default:
					session.SendParameterError();
					return;
			}

			session.SendResponse(Resource.AuthOk_234, ssl);
			
			session.SocketSession.InitStream(session.Context);
		}

		#endregion
	}
}
