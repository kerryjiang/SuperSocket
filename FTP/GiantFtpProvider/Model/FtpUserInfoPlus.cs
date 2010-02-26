using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.FtpService;

namespace GiantFtpProvider.Model
{
	public partial class FtpUserInfo
	{
		public FtpUser ToFtpUser()
		{
			FtpUser	user	= new FtpUser();
			
			user.UserID		= this.userID;
			user.UserName	= this.userName;
			user.MaxSpace	= this.maxSpace;
			user.MaxSpeed	= this.maxSpeed;
			user.MaxThread	= this.maxThread;
			user.UsedSpace	= this.usedSpace;
			user.Password	= this.password;
			
			return user;		
		}
	}
}
