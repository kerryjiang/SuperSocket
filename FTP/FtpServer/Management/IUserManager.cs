using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using GiantSoft.SocketServiceCore;

namespace GiantSoft.FtpService.Management
{
	[ServiceContract]
	public interface IUserManager
	{
		[OperationContract]
		List<FtpUser> GetAllUsers();

		[OperationContract]
		FtpUser GetFtpUserByID(long userID);

		[OperationContract]
		FtpUser GetFtpUserByUserName(string username);

		[OperationContract]
		bool UpdateFtpUser(FtpUser user);

		[OperationContract]
		CreateUserResult CreateFtpUser(FtpUser user);

		[OperationContract]
		bool ChangePassword(long userID, string password);
	}
}
