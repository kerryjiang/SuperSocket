using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.FtpService.Membership
{
	/// <summary>
	/// Anonymous ftp user
	/// </summary>
	public class Anonymous : FtpUser
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Anonymous"/> class.
		/// </summary>
		public Anonymous()
		{
			this.UserName	= "anonymous";
			this.UserID		= 0;
			this.MaxThread	= 2;
		}
	}
}
