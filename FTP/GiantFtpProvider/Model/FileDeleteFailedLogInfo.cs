//------------------------------------------------------------------------------
//     Date:    11/18/2007
//     Time:    5:25 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table FileDeleteFailedLog
	/// </summary>
	[Serializable]
	[MapTable("FileDeleteFailedLogs")]
	public partial class FileDeleteFailedLogInfo : DataEntry
	{
		
		
		private long logID;
		
		[MapColumn("LogID", DbType.Int64, true, false)]
		public long LogID
		{
			get { return logID; }
			set { logID = value; }			
		}
		
		private long hashID;
		
		[MapColumn("HashID", DbType.Int64)]
		public long HashID
		{
			get { return hashID; }
			set { hashID = value; RefreshUpdateColumn("HashID"); }			
		}
		
		private long userID;
		
		[MapColumn("UserID", DbType.Int64)]
		public long UserID
		{
			get { return userID; }
			set { userID = value; RefreshUpdateColumn("UserID"); }			
		}
		
		private string userPath;
		
		[MapColumn("UserPath", DbType.String)]
		public string UserPath
		{
			get { return userPath; }
			set { userPath = value; RefreshUpdateColumn("UserPath"); }			
		}
		
		private string storagePath;
		
		[MapColumn("StoragePath", DbType.String)]
		public string StoragePath
		{
			get { return storagePath; }
			set { storagePath = value; RefreshUpdateColumn("StoragePath"); }			
		}
		
		private DateTime deleteTime;
		
		[MapColumn("DeleteTime", DbType.DateTime)]
		public DateTime DeleteTime
		{
			get { return deleteTime; }
			set { deleteTime = value; RefreshUpdateColumn("DeleteTime"); }			
		}

	}
}

