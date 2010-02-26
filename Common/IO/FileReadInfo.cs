using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.Common.IO
{
	public class FileReadInfo
	{
		public DateTime LastModifyTime { get; set; }

		public long PrevLength { get; set; }

		public FileContentAppendEventHandler Handler { get; set; }

		public object SyncRoot = new object();
	}
}
