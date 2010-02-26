using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GiantSoft.FtpService.Membership;
using GiantSoft.SocketServiceCore;
using GiantSoft.FtpService;

namespace GiantSoft.SuperFtpProvider
{
	class SuperMembershipProvider : MembershipProviderBase
	{
		public override AuthenticationResult Authenticate(string username, string password, out GiantSoft.FtpService.FtpUser user)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUsedSpaceForUser(FtpUser user)
		{
			throw new NotImplementedException();
		}

		public override void OnServiceStop()
		{
			throw new NotImplementedException();
		}
	}
}
