//------------------------------------------------------------------------------
//     Date:    11/18/2007
//     Time:    2:40 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table HashMap
	/// </summary>
	[Serializable]
	[MapTable("HashMap")]
	public partial class HashMapInfo : DataEntry
	{
		
		
		private long hashID;
		
		[MapColumn("HashID", DbType.Int64, true, false)]
		public long HashID
		{
			get { return hashID; }
			set { hashID = value; }			
		}
		
		private string hashCode;
		
		[MapColumn("HashCode", DbType.String)]
		public string HashCode
		{
			get { return hashCode; }
			set { hashCode = value; RefreshUpdateColumn("HashCode"); }			
		}
		
		private int storageID;
		
		[MapColumn("StorageID", DbType.Int32)]
		public int StorageID
		{
			get { return storageID; }
			set { storageID = value; RefreshUpdateColumn("StorageID"); }			
		}

	}
}

