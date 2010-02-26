using System;
using System.Collections.Generic;
using System.Text;

namespace GiantSoft.FtpService.Storage
{
	public enum RenameResult
	{
		UnknownError,
		SameNameFolderExist,
		SameNameFileExist,
		Success
	}
}
