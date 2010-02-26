using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GiantSoft.FtpService.Storage;
using GiantSoft.FtpService;

namespace GiantSoft.SuperFtpProvider
{
	class SuperStorageProvider : StorageProviderBase
	{
		public override List<ListItem> GetList(FtpContext context)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteFile(FtpContext context, string filename)
		{
			throw new NotImplementedException();
		}

		public override DateTime GetModifyTime(FtpContext context, string filename)
		{
			throw new NotImplementedException();
		}

		public override long GetFileSize(FtpContext context, string filename)
		{
			throw new NotImplementedException();
		}

		public override bool RenameFile(FtpContext context, string oldPath, string newPath)
		{
			throw new NotImplementedException();
		}

		public override bool RenameFolder(FtpContext context, string oldPath, string newPath)
		{
			throw new NotImplementedException();
		}

		public override bool IsExistFolder(FtpContext context, string folderpath, out long folderID)
		{
			throw new NotImplementedException();
		}

		public override bool IsExistFile(FtpContext context, string filepath)
		{
			throw new NotImplementedException();
		}

		public override bool CreateFolder(FtpContext context, string foldername)
		{
			throw new NotImplementedException();
		}

		public override bool RemoveFolder(FtpContext context, string foldername)
		{
			throw new NotImplementedException();
		}

		public override long GetUserRealUsedSpace(FtpContext context, string username)
		{
			throw new NotImplementedException();
		}

		public override string GetTempDirectory(string sessionID)
		{
			throw new NotImplementedException();
		}

		public override void ClearTempDirectory(FtpContext context)
		{
			throw new NotImplementedException();
		}

		public override void OnServiceStop()
		{
			throw new NotImplementedException();
		}
	}
}
