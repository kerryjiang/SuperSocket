using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.FtpService.Command
{
	public class STOU : STOR
	{
		public override void Execute(FtpSession session, CommandInfo commandData)
		{
			if (string.IsNullOrEmpty(commandData.Param))
			{
				commandData.Param = Guid.NewGuid().ToString();
			}
			
			base.Execute(session, commandData);
		}
	}
}
