using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.FtpService.Membership
{
	/// <summary>
	/// Password format
	/// </summary>
	public enum PasswordFormat
	{
		/// <summary>
		/// Plain text
		/// </summary>
		Plain,
		/// <summary>
		/// Encrypted by MD4
		/// </summary>
		MD4,
		/// <summary>
		/// Encrypted by MD5
		/// </summary>
		MD5
	}
}
