using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.FtpService.Storage
{
	/// <summary>
	/// The option for storing file
	/// </summary>
	public class StoreOption
	{
		private bool appendOriginalFile = true;

		/// <summary>
		/// Gets or sets a value indicating whether [append original file].
		/// </summary>
		/// <value><c>true</c> if [append original file]; otherwise, <c>false</c>.</value>
		public bool AppendOriginalFile
		{
			get { return appendOriginalFile; }
			set { appendOriginalFile = value; }
		}
		
		private long totalRead = 0;

		/// <summary>
		/// Gets or sets the total read.
		/// </summary>
		/// <value>The total read.</value>
		public long TotalRead
		{
			get { return totalRead; }
			set { totalRead = value; }
		}
	}
}
