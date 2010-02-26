using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GiantFtpProvider.Model;
using GiantSoft.FtpService.Storage;
using GiantSoft.FtpService;

namespace GiantFtpProvider.Service
{
	interface IDataAccess
	{
		FtpUserInfo GetUserByName(string username);		

		long GetFolderIDByName(long userID, long parentID, string folderName);

		long GetFolderIDByPath(long userID, string folderPath);
		
		long GetFolderIDByRelativePath(long userID, long parentID, string relativePath);
		
		long GetFileIDByName(long userID, long parentID, string fileName);
		
		bool GetFileStorageInfo(long userID, long parentID, string fileName, out long fileID, out long hashID, out string storageDir);
		
		bool GetHashStorageInfo(long hashID, out string storageDir, out string hashCode);

		bool DeleteFile(long userID, long parentID, string filename, out long fileID, out long hashID, out string storageDir, out bool deletePhysicalFile);
		
		void LogDeleteFileFail(long hashID, long userID, string userPath, string storagePath);
		
		Model.FileInfo GetFileInfoByName(long userID, long parentID, string fileName);

		Model.FolderInfo GetFolderInfoByName(long userID, long parentID, string folderName);

		Model.FolderInfo GetFolderInfoByID(long userID, long folderID);
		
		DataTable GetSubFiles(long userID, long parentID);

		DataTable GetSubFolders(long userID, long parentID);
		
		RenameResult RenameFile(long userID, long fileID, string newFileName);

		RenameResult RenameFolder(long userID, long folderID, string newFolderName);
		
		bool DeleteFolder(long userID, long parentID, string folderName);
		
		bool CreateFolder(long userID, long parentID, string folderName);
		
		long GetUserUsedSpace(long userID);

		long GetUserUsedSpace(string username);
		
		StorageSetInfo GetLastStorageSet();

		bool UploadNewFile(Model.FileInfo file, string hashCode, out long hashID, out string storageDir);

		bool AppendNewFile(Model.FileInfo file, string hashCode,out long hashID, out bool deleteOldFile, out string storageDir);
		
		void SaveUserUsedSpace(FtpUser user, long usedSpace);
		
		List<SystemFolderInfo> GetSystemFolders();
		
	}
}
