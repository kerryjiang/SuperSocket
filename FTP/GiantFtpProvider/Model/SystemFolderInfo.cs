//------------------------------------------------------------------------------
//     Date:    1/5/2008
//     Time:    9:32 PM
//     Version: 4.0.0.0
//------------------------------------------------------------------------------

using System;
using System.Data;
using GiantSoft.Orm;

namespace GiantFtpProvider.Model
{
	/// <summary>
	/// Entity Class for Table SystemFolders
	/// </summary>
	[Serializable]
	[MapTable("SystemFolders")]
	public partial class SystemFolderInfo : DataEntry
	{
		
		
		private string folderPath;
		
		[MapColumn("FolderPath", DbType.String, true, false)]
		public string FolderPath
		{
			get { return folderPath; }
			set { folderPath = value; }			
		}
		
		private string description;
		
		[MapColumn("Description", DbType.String)]
		public string Description
		{
			get { return description; }
			set { description = value; RefreshUpdateColumn("Description"); }			
		}

	}
}

