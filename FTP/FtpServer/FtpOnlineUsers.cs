using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.FtpService
{
	public static class FtpOnlineUsers
	{
		private static object syncRoot = new object();

		private static Dictionary<long, FtpUser> dictOnlineUser = new Dictionary<long, FtpUser>();

		public static bool Logon(FtpContext context, FtpUser user)
		{
			FtpUser onlineUser = null;
			
			if (dictOnlineUser.TryGetValue(user.UserID, out onlineUser))
			{
				context.User	= onlineUser;
				return onlineUser.IncreaseThread();
			}
			else
			{
				user.IncreaseThread();
				context.User = user;

				lock (syncRoot)
				{
					dictOnlineUser[user.UserID] = user;
				}

				return true;
			}
		}

		public static void RemoveOnlineUser(FtpSession session, FtpUser user)
		{
			FtpUser onlineUser = null;

			if (dictOnlineUser.TryGetValue(user.UserID, out onlineUser))
			{
				if (onlineUser.DecreaseThread() <= 0)
				{
					lock (syncRoot)
					{
						dictOnlineUser.Remove(user.UserID);
					}

					session.FtpServiceProvider.UpdateUsedSpaceForUser(user);
				}
			}
		}
	}
}
