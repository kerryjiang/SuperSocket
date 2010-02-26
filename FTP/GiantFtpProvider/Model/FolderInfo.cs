//------------------------------------------------------------------------------
//     Date:    12/2/2007
//     Time:    5:55 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table Folders
	/// </summary>
	[Serializable]
	[MapTable("Folders")]
	public partial class FolderInfo : DataEntry
	{
		
		
		private long folderID;
		
		[MapColumn("FolderID", DbType.Int64, true, false)]
		public long FolderID
		{
			get { return folderID; }
			set { folderID = value; }			
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
		
		private string folderName;
		
		[MapColumn("FolderName", DbType.String)]
		public string FolderName
		{
			get { return folderName; }
			set { folderName = value; RefreshUpdateColumn("FolderName"); }			
		}		
		
		private int subFileCount;
		
		[MapColumn("SubFileCount", DbType.Int32)]
		public int SubFileCount
		{
			get { return subFileCount; }
			set { subFileCount = value; RefreshUpdateColumn("SubFileCount"); }			
		}
		
		private int subFolderCount;
		
		[MapColumn("SubFolderCount", DbType.Int32)]
		public int SubFolderCount
		{
			get { return subFolderCount; }
			set { subFolderCount = value; RefreshUpdateColumn("SubFolderCount"); }			
		}
		
		private long folderSize;
		
		[MapColumn("FolderSize", DbType.Int64)]
		public long FolderSize
		{
			get { return folderSize; }
			set { folderSize = value; RefreshUpdateColumn("FolderSize"); }			
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

	}
}

