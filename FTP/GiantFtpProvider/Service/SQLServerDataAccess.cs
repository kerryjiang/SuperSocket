using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using GiantSoft.Common;
using GiantSoft.Orm;
using GiantSoft.FtpService;
using GiantSoft.FtpService.Storage;
using GiantFtpProvider.Model;



namespace GiantFtpProvider.Service
{
	class SQLServerDataAccess : IDataAccess
	{
		#region IDataAccess Members

		public FtpUserInfo GetUserByName(string username)
		{			
			if (string.IsNullOrEmpty(username))
				throw new ArgumentNullException("GetUserByName with null argument");
				
			FtpUserInfo user	= new FtpUserInfo();
			user.LowerUserName	= username.ToLower();
			
			if(user.LoadObjectByColumn("LowerUserName"))
				return user;
			else
				return null;			
		}

		public long GetFolderIDByName(long userID, long parentID, string folderName)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetSqlStringCommand("select FolderID from Folders with(nolock) where UserID=@UserID and ParentID=@ParentID and FolderName=@FolderName");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@ParentID", DbType.Int64, parentID);
			database.AddInParameter(command, "@FolderName", DbType.String, folderName);
			object result = database.ExecuteScalar(command);

			if (result == null)
				return 0;
			else
				return StringUtil.ParseLong(result.ToString());
		}

		public long GetFolderIDByPath(long userID, string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath) || folderPath=="/")
			{
				return 0;
			}
			
			if(folderPath.Length<1000)
			{
				return GetFolderIDByRelativePath(userID, 0, folderPath);
			}	
			else
			{
				int startPos = 0;
				int endPos = 1000;
				int pos = 999;
				
				long parentID	= 0;
				string parentPath	= string.Empty;
				
				while(endPos<folderPath.Length)
				{
					while (pos > startPos && folderPath[pos] != '/')
					{
						pos--;
					}

					parentPath	= folderPath.Substring(startPos, pos);
					parentID	= GetFolderIDByRelativePath(userID, parentID, parentPath);
					if(parentID<=0)
						return -1;
						
					startPos	= pos;
					endPos		= startPos + 1000;
					pos			= endPos;
					
					if(endPos> folderPath.Length-1)
					{
						parentID = GetFolderIDByRelativePath(userID, parentID, folderPath.Substring(startPos));
						if(parentID>0)
							return parentID;
						else
							return -1;
					}
												
				}
				
				return -1;						
			}		
		}

		public long GetFolderIDByRelativePath(long userID, long parentID, string relativePath)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("GetFolderIDByRelativePath");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@ParentID", DbType.Int64, parentID);
			database.AddInParameter(command, "@RelativePath", DbType.String, relativePath);
			object result = database.ExecuteScalar(command);

			if (result == null)
				return 0;
			else
				return StringUtil.ParseLong(result.ToString());
			
		}

		public long GetFileIDByName(long userID, long parentID, string fileName)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetSqlStringCommand("select FileID from Files with(nolock) where UserID=@UserID and ParentID=@ParentID and FileName=@FileName");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@ParentID", DbType.Int64, parentID);
			database.AddInParameter(command, "@FileName", DbType.String, fileName);
			object result = database.ExecuteScalar(command);

			if (result == null)
				return 0;
			else
				return StringUtil.ParseLong(result.ToString());
		}

		public bool GetFileStorageInfo(long userID, long parentID, string fileName, out long fileID, out long hashID, out string storageDir)
		{			
			storageDir = string.Empty;
			hashID = 0;
			fileID = GetFileIDByName(userID, parentID, fileName);
			
			if(fileID<=0)
				return false;

			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("GetFileStorePath");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@FileID", DbType.Int64, fileID);

			DataSet ds = database.ExecuteDataSet(command);

			if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
				return false;
			else
			{
				storageDir = "" + ds.Tables[0].Rows[0][1];
				hashID = StringUtil.ParseLong("" + ds.Tables[0].Rows[0][2]);
				return true;
			}
		}

		public bool DeleteFile(long userID, long parentID, string filename, out long fileID, out long hashID, out string storageDir, out bool deletePhysicalFile)
		{			
			storageDir = string.Empty;
			hashID = 0;
			deletePhysicalFile = false;

			fileID = GetFileIDByName(userID, parentID, filename);
			
			if(fileID<=0)
				return false;

			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("DeleteFile");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@FileID", DbType.Int64, fileID);

			DataSet ds = database.ExecuteDataSet(command);

			if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
				return false;
			else
			{
				storageDir = "" + ds.Tables[0].Rows[0][0];
				hashID = StringUtil.ParseLong("" + ds.Tables[0].Rows[0][1]);
				deletePhysicalFile = ds.Tables[0].Rows[0][2].ToString().Equals("1");
				return true;
			}
		}
		
		public void LogDeleteFileFail(long hashID, long userID, string userPath, string storagePath)
		{
			FileDeleteFailedLogInfo log	= new FileDeleteFailedLogInfo();
			
			log.HashID	= hashID;
			log.UserID	= userID;
			log.UserPath	= userPath;
			log.StoragePath	= storagePath;
			log.DeleteTime	= DateTime.Now;
			
			try
			{			
				log.Insert();
			}
			catch(Exception e)
			{
				LogUtil.LogError(e);
			}
		}

		public FileInfo GetFileInfoByName(long userID, long parentID, string fileName)
		{
			long fileID		= GetFileIDByName(userID, parentID, fileName);
			
			if(fileID<=0)
				return null;
				
			Model.FileInfo file = new Model.FileInfo();
			file.FileID			= fileID;
			if(file.LoadObjectByKey())
				return file;
			else
				return null;		
		}

		public DataTable GetSubFiles(long userID, long parentID)
		{
			Model.FileInfo file	= new Model.FileInfo();
			file.UserID			= userID;
			file.ParentID		= parentID;
			return DataEntry.SearchList<Model.FileInfo>(file);
		}

		public DataTable GetSubFolders(long userID, long parentID)
		{
			FolderInfo folder	= new FolderInfo();
			folder.UserID		= userID;
			folder.ParentID		= parentID;			
			return DataEntry.SearchList<FolderInfo>(folder);
		}

		public RenameResult RenameFile(long userID, long fileID, string newFileName)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("RenameFile");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@FileID", DbType.Int64, fileID);
			database.AddInParameter(command, "@NewFileName", DbType.String, newFileName);

			string result = "" + database.ExecuteScalar(command);
			
			switch(result)
			{
				case("0"):
					return RenameResult.Success;
				case("1"):
					return RenameResult.SameNameFileExist;
				case("2"):
					return RenameResult.SameNameFolderExist;
				default:
					return RenameResult.UnknownError;
			}
			
		}

		public FolderInfo GetFolderInfoByName(long userID, long parentID, string folderName)
		{
			long folderID	= GetFolderIDByName(userID, parentID, folderName);
			
			if(folderID<=0)
				return null;
				
			return GetFolderInfoByID(userID, folderID);
		}

		public FolderInfo GetFolderInfoByID(long userID, long folderID)
		{
			Model.FolderInfo folder = new Model.FolderInfo();
			folder.FolderID = folderID;
			if (folder.LoadObjectByKey())
				return folder;
			else
				return null;
		}

		public RenameResult RenameFolder(long userID, long folderID, string newFolderName)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("RenameFolder");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@FolderID", DbType.Int64, folderID);
			database.AddInParameter(command, "@NewFolderName", DbType.String, newFolderName);

			string result = "" + database.ExecuteScalar(command);

			switch (result)
			{
				case ("0"):
					return RenameResult.Success;
				case ("1"):
					return RenameResult.SameNameFileExist;
				case ("2"):
					return RenameResult.SameNameFolderExist;
				default:
					return RenameResult.UnknownError;
			}
		}

		public bool DeleteFolder(long userID, long parentID, string folderName)
		{			
			long folderID	= GetFolderIDByName(userID, parentID, folderName);
			
			if(folderID<=0)
				return false;
			
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("DeleteFolder");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@FolderID", DbType.Int64, folderID);

			string result = "" + database.ExecuteScalar(command);

			switch (result)
			{
				case ("0"):
					return true;
				case ("1"):
					return false;
				case ("2"):
					return false;
				default:
					return false;
			}
		}

		public bool CreateFolder(long userID, long parentID, string folderName)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("CreateFolder");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@ParentID", DbType.Int64, parentID);
			database.AddInParameter(command, "@FolderName", DbType.String, folderName);

			string result = "" + database.ExecuteScalar(command);

			switch (result)
			{
				case ("0"):
					return true;
				case ("1"):
					return false;
				case ("2"):
					return false;
				default:
					return false;
			}
		}

		public long GetUserUsedSpace(long userID)
		{
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("GetUserUsedSpace");
			database.AddInParameter(command, "@UserID", DbType.Int64, userID);
			database.AddInParameter(command, "@UserName", DbType.String, string.Empty);
			return StringUtil.ParseLong("" + database.ExecuteScalar(command));
		}

		public long GetUserUsedSpace(string username)
		{
			if(string.IsNullOrEmpty(username))
				throw new ArgumentNullException("GetUserUsedSpace null argument");
				
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("GetUserUsedSpace");
			database.AddInParameter(command, "@UserID", DbType.Int64, 0);
			database.AddInParameter(command, "@UserName", DbType.String, username);
			return StringUtil.ParseLong("" + database.ExecuteScalar(command));
		}

		public bool GetHashStorageInfo(long hashID, out string storageDir, out string hashCode)
		{
			storageDir	= string.Empty;
			hashCode	= string.Empty;
			
			if(hashID<=0)
				return false;

			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("GetHashStorePath");
			database.AddInParameter(command, "@HashID", DbType.Int64, hashID);
					
			DataSet ds = database.ExecuteDataSet(command);

			if (ds == null || ds.Tables.Count <= 0 || ds.Tables[0].Rows.Count <= 0)
				return false;
			else
			{
				storageDir	= "" + ds.Tables[0].Rows[0][0];
				hashCode	= "" + ds.Tables[0].Rows[0][1];
				return true;	
			}
		}

		private DateTime m_ExpireTime = DateTime.Now;
		
		private StorageSetInfo m_LastStorageSet = null;		
		
		public StorageSetInfo GetLastStorageSet()
		{
			if(m_LastStorageSet==null || DateTime.Now > m_ExpireTime)
			{
				lock(typeof(SQLServerDataAccess))
				{
					StorageSetInfo set	= new StorageSetInfo();
					set.Enabled			= true;
					m_LastStorageSet	= DataEntry.GetTopObject<StorageSetInfo>(set);
					m_ExpireTime		= DateTime.Now.AddMinutes(15);//Cache for 15 mins ?
				}
			}			
			return m_LastStorageSet;
		}

		public bool UploadNewFile(FileInfo file, string hashCode, out long hashID, out string storageDir)
		{
			hashID = 0;
			storageDir = string.Empty;
			
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("UploadNewFile");
			
			database.AddInParameter(command, "@UserID", DbType.Int64, file.UserID);
			database.AddInParameter(command, "@ParentID", DbType.Int64, file.ParentID);
			database.AddInParameter(command, "@FileName", DbType.String, file.FileName);
			database.AddInParameter(command, "@FileSize", DbType.Int64, file.FileSize);
			database.AddInParameter(command, "@TempHash", DbType.String, file.TempHash);
			database.AddInParameter(command, "@HashCode", DbType.String, hashCode);
			database.AddOutParameter(command, "@FileID", DbType.Int64, 4);
			database.AddOutParameter(command, "@HashID", DbType.Int64, 4);
			database.AddOutParameter(command, "@StoreDir", DbType.String, 256);
			
			
			if(database.ExecuteNonQuery(command)<=0)
			{
				return false;
			}

			long fileID = StringUtil.ParseLong("" + database.GetParameterValue(command, "@FileID"));
			hashID		= StringUtil.ParseLong("" + database.GetParameterValue(command, "@HashID"));
			storageDir	= "" + database.GetParameterValue(command, "@StoreDir");
			
			return fileID>0 && hashID>0 && !string.IsNullOrEmpty(storageDir);
		}

		public bool AppendNewFile(FileInfo file, string hashCode, out long hashID, out bool deleteOldFile, out string storageDir)
		{
			hashID	= 0;
			deleteOldFile = false;
			storageDir = string.Empty;
			
			Database database = DatabaseFactory.CreateDatabase();
			DbCommand command = database.GetStoredProcCommand("AppendNewFile");

			database.AddInParameter(command, "@UserID", DbType.Int64, file.UserID);
			database.AddInParameter(command, "@FileID", DbType.Int64, file.FileID);
			database.AddInParameter(command, "@FileSize", DbType.Int64, file.FileSize);
			database.AddInParameter(command, "@TempHash", DbType.String, file.TempHash);
			database.AddInParameter(command, "@HashCode", DbType.String, hashCode);			
			database.AddOutParameter(command, "@HashID", DbType.Int64, 4);
			database.AddOutParameter(command, "@Delete", DbType.Boolean, 1);
			database.AddOutParameter(command, "@StoreDir", DbType.String, 256);

			if (database.ExecuteNonQuery(command) <= 0)
			{
				return false;
			}

			hashID = StringUtil.ParseLong("" + database.GetParameterValue(command, "@HashID"));

			if (!bool.TryParse("" + database.GetParameterValue(command, "@Delete"), out deleteOldFile))
				deleteOldFile = false;
				
			storageDir = "" + database.GetParameterValue(command, "@StoreDir");

			return hashID > 0 && !string.IsNullOrEmpty(storageDir);		
		}

		public void SaveUserUsedSpace(FtpUser user, long usedSpace)
		{
			if(usedSpace<0)
				throw new ArgumentException("Used space can not be minus.");
				
			FtpUserInfo updateUser	= new FtpUserInfo();
			updateUser.UserID		= user.UserID;
			updateUser.UsedSpace	= usedSpace;
			updateUser.UpdateObjectByKey();			
		}		

		public List<SystemFolderInfo> GetSystemFolders()
		{
			return DataEntry.GetAllObjects<SystemFolderInfo>();
		}

		#endregion
	}
}
