//------------------------------------------------------------------------------
//     Date:    11/20/2007
//     Time:    10:17 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table Files
	/// </summary>
	[Serializable]
	[MapTable("Files")]
	public partial class FileInfo : DataEntry
	{
		
		
		private long fileID;
		
		[MapColumn("FileID", DbType.Int64, true, false)]
		public long FileID
		{
			get { return fileID; }
			set { fileID = value; }			
		}
		
		private long userID;
		
		[MapColumn("UserID", DbType.Int64)]
		public long UserID
		{
			get { return userID; }
			set { userID = value; RefreshUpdateColumn("UserID"); }			
		}
		
		private long parentID;
		
		[MapColumn("ParentID", DbType.Int64)]
		public long ParentID
		{
			get { return parentID; }
			set { parentID = value; RefreshUpdateColumn("ParentID"); }			
		}
		
		private string fileName;
		
		[MapColumn("FileName", DbType.String)]
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; RefreshUpdateColumn("FileName"); }			
		}
		
		private long fileSize;
		
		[MapColumn("FileSize", DbType.Int64)]
		public long FileSize
		{
			get { return fileSize; }
			set { fileSize = value; RefreshUpdateColumn("FileSize"); }			
		}
		
		private DateTime createTime;
		
		[MapColumn("CreateTime", DbType.DateTime)]
		public DateTime CreateTime
		{
			get { return createTime; }
			set { createTime = value; RefreshUpdateColumn("CreateTime"); }			
		}
		
		private DateTime updateTime;
		
		[MapColumn("UpdateTime", DbType.DateTime)]
		public DateTime UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; RefreshUpdateColumn("UpdateTime"); }			
		}
		
		private string tempHash;
		
		[MapColumn("TempHash", DbType.String)]
		public string TempHash
		{
			get { return tempHash; }
			set { tempHash = value; RefreshUpdateColumn("TempHash"); }			
		}
		
		private long hashID;
		
		[MapColumn("HashID", DbType.Int64)]
		public long HashID
		{
			get { return hashID; }
			set { hashID = value; RefreshUpdateColumn("HashID"); }			
		}

	}
}

