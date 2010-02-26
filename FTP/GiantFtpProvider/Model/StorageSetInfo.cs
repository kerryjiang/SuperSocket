//------------------------------------------------------------------------------
//     Date:    12/2/2007
//     Time:    4:14 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table StorageSets
	/// </summary>
	[Serializable]
	[MapTable("StorageSets")]
	public partial class StorageSetInfo : DataEntry
	{
		
		
		private int storageID;
		
		[MapColumn("StorageID", DbType.Int32, true, false)]
		public int StorageID
		{
			get { return storageID; }
			set { storageID = value; }			
		}
		
		private string storagePath;
		
		[MapColumn("StoragePath", DbType.String)]
		public string StoragePath
		{
			get { return storagePath; }
			set { storagePath = value; RefreshUpdateColumn("StoragePath"); }			
		}
		
		private bool enabled;
		
		[MapColumn("Enabled", DbType.Boolean)]
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; RefreshUpdateColumn("Enabled"); }			
		}

	}
}

