using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.FtpService.Storage
{
	public enum RenameResult
	{
		UnknownError,
		SameNameFolderExist,
		SameNameFileExist,
		Success
	}
}
