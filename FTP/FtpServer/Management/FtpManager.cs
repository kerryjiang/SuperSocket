using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SuperSocket.SocketServiceCore;

namespace SuperSocket.FtpService.Management
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	public class FtpManager : IServerManager, IStatusReporter, IUserManager
	{
		private FtpServer m_Server = null;

		private FtpManager()
		{

		}

		public FtpManager(FtpServer server)
		{
			m_Server = server;
		}

		#region IStatusReporter Members

		public int GetCurrentConnectionCount()
		{
			return m_Server.SessionCount;
		}

		public int GetOnlineUserCount()
		{
			return 0;
		}

		public string Ping()
		{
			return Guid.NewGuid().ToString();
		}

		#endregion

		#region IServerManager Members

		public void Start()
		{
			m_Server.Start();
		}

		public void Stop()
		{
			m_Server.Stop();
		}

		public void Restart()
		{
			m_Server.Stop();
			m_Server.Start();
		}

		#endregion

		#region IUserManager Members

		public List<FtpUser> GetAllUsers()
		{
			return m_Server.FtpServiceProvider.GetAllUsers();
		}

		public FtpUser GetFtpUserByID(long userID)
		{
			return m_Server.FtpServiceProvider.GetFtpUserByID(userID);
		}

		public FtpUser GetFtpUserByUserName(string username)
		{
			return m_Server.FtpServiceProvider.GetFtpUserByUserName(username);
		}

		public bool UpdateFtpUser(FtpUser user)
		{
			return m_Server.FtpServiceProvider.UpdateFtpUser(user);
		}

		public CreateUserResult CreateFtpUser(FtpUser user)
		{
			return m_Server.FtpServiceProvider.CreateFtpUser(user);
		}

		public bool ChangePassword(long userID, string password)
		{
			return m_Server.FtpServiceProvider.ChangePassword(userID, password);
		}

		#endregion
	}
}
