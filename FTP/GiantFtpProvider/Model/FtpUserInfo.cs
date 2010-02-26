//------------------------------------------------------------------------------
//     Date:    11/18/2007
//     Time:    2:11 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table FtpUsers
	/// </summary>
	[Serializable]
	[MapTable("FtpUsers")]
	public partial class FtpUserInfo : DataEntry
	{
		
		
		private long userID;
		
		[MapColumn("UserID", DbType.Int64, true, false)]
		public long UserID
		{
			get { return userID; }
			set { userID = value; }			
		}
		
		private string userName;
		
		[MapColumn("UserName", DbType.String)]
		public string UserName
		{
			get { return userName; }
			set { userName = value; RefreshUpdateColumn("UserName"); }			
		}
		
		private string password;
		
		[MapColumn("Password", DbType.String)]
		public string Password
		{
			get { return password; }
			set { password = value; RefreshUpdateColumn("Password"); }			
		}
		
		private string email;
		
		[MapColumn("Email", DbType.String)]
		public string Email
		{
			get { return email; }
			set { email = value; RefreshUpdateColumn("Email"); }			
		}
		
		private long maxSpace;
		
		[MapColumn("MaxSpace", DbType.Int64)]
		public long MaxSpace
		{
			get { return maxSpace; }
			set { maxSpace = value; RefreshUpdateColumn("MaxSpace"); }			
		}
		
		private long usedSpace;
		
		[MapColumn("UsedSpace", DbType.Int64)]
		public long UsedSpace
		{
			get { return usedSpace; }
			set { usedSpace = value; RefreshUpdateColumn("UsedSpace"); }			
		}
		
		private int maxThread;
		
		[MapColumn("MaxThread", DbType.Int32)]
		public int MaxThread
		{
			get { return maxThread; }
			set { maxThread = value; RefreshUpdateColumn("MaxThread"); }			
		}
		
		private int maxSpeed;
		
		[MapColumn("MaxSpeed", DbType.Int32)]
		public int MaxSpeed
		{
			get { return maxSpeed; }
			set { maxSpeed = value; RefreshUpdateColumn("MaxSpeed"); }			
		}
		
		private string lowerUserName;
		
		[MapColumn("LowerUserName", DbType.String)]
		public string LowerUserName
		{
			get { return lowerUserName; }
			set { lowerUserName = value; RefreshUpdateColumn("LowerUserName"); }			
		}
		
		private string lowerEmail;
		
		[MapColumn("LowerEmail", DbType.String)]
		public string LowerEmail
		{
			get { return lowerEmail; }
			set { lowerEmail = value; RefreshUpdateColumn("LowerEmail"); }			
		}

	}
}

