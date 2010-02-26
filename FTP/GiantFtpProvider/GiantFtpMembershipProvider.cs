using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.FtpService;
using GiantSoft.FtpService.Membership;
using GiantSoft.SocketServiceCore;
using GiantFtpProvider.Model;
using GiantFtpProvider.Service;

namespace GiantFtpProvider
{
	public sealed class GiantFtpMembershipProvider : MembershipProviderBase
	{		
		public override AuthenticationResult Authenticate(string username, string password, out FtpUser user)
		{
			user = null;

			FtpUserInfo userInfo = ServiceProvider.DAInstance.GetUserByName(username);

			if (userInfo==null)
				return AuthenticationResult.NotExist;
			else
			{
				user = userInfo.ToFtpUser();
			}
			
			//Compare password
			if (string.Compare(password, user.Password, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return AuthenticationResult.Success;
			}
			else
			{
				return AuthenticationResult.PasswordError;
			}		
		}

		public override void UpdateUsedSpaceForUser(FtpUser user)
		{
			long usedSpace	= ServiceProvider.DAInstance.GetUserUsedSpace(user.UserID);
			
			ServiceProvider.DAInstance.SaveUserUsedSpace(user, usedSpace);
		}

		public override void OnServiceStop()
		{
			//do nothing
		}
	}
}
