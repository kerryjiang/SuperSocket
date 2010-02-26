using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;

namespace GiantSoft.FtpService.Command
{
	class TYPE : ICommand<FtpSession>
	{
		#region ICommand<FtpSession> Members

		public void Execute(FtpSession session, CommandInfo commandData)
		{
			char typeCode = ' ';
			
			string newType = commandData.GetFirstParam();

			if (!string.IsNullOrEmpty(newType) && newType.Length>0)
			{
				typeCode = newType[0];
			}

			switch (typeCode)
			{
				case ('A'):
					session.Context.TransferType = TransferType.A;
					break;
				case ('E'):
					session.Context.TransferType = TransferType.E;
					break;
				case ('I'):
					session.Context.TransferType = TransferType.I;
					break;
				case ('L'):
					session.Context.TransferType = TransferType.L;
					break;
				default:
					session.SendParameterError();
					return;
			}

			session.SendResponse(Resource.TypeOk_220, typeCode);	
		}

		#endregion
	}
}
