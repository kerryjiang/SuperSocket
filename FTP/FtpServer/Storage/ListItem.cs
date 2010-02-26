using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.FtpService.Storage
{
	public class ListItem
	{
		private ItemType itemType = ItemType.File;

		public ItemType ItemType
		{
			get { return itemType; }
			set { itemType = value; }
		}

		private string permission;

		public string Permission
		{
			get { return permission; }
			set { permission = value; }
		}

		private string name;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private string ownerName;

		public string OwnerName
		{
			get { return ownerName; }
			set { ownerName = value; }
		}

		private long length;

		public long Length
		{
			get { return length; }
			set { length = value; }
		}

		private DateTime lastModifiedTime;

		public DateTime LastModifiedTime
		{
			get { return lastModifiedTime; }
			set { lastModifiedTime = value; }
		}

	}
}
