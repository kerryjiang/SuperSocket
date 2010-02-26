using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using GiantFtpProvider.Model;
using GiantSoft.Common;
using GiantSoft.Orm;
using GiantSoft.Common.Hash;
using GiantSoft.FtpService;
using GiantSoft.FtpService.Storage;
using GiantFtpProvider.Service;

namespace GiantFtpProvider
{
	public class GiantFtpStorageProvider : StorageProviderBase
	{
			
		public override List<ListItem> GetList(FtpContext context)
		{
			string path		= context.CurrentPath;
			
			long folderID	= context.CurrentFolderID;
			
			//if(this.IsRootPath(path))
			//    folderID = 0;
			//else
			//{
			//    folderID = ServiceProvider.DAInstance.GetFolderIDByPath(context.UserID, path);
				
			//    if(folderID<=0)
			//        throw new DirectoryNotFoundException(context.CurrentPath);						
			//}
			
			List<ListItem> list	= new List<ListItem>();
			
			using(DataTable dtFolder = ServiceProvider.DAInstance.GetSubFolders(context.UserID, folderID))
			{
				if(dtFolder!=null && dtFolder.Rows.Count>0)
				{
					DataRow row	= null;
					
					for(int i = 0; i< dtFolder.Rows.Count; i++)
					{
						row	= dtFolder.Rows[i];
						ListItem item	= new ListItem();
						item.ItemType	= ItemType.Folder;
						item.LastModifiedTime = StringUtil.ParseDateTime("" + row["UpdateTime"]);
						item.Length = StringUtil.ParseLong("" + row["FolderSize"]);
						item.Name	= "" + row["FolderName"];
						item.Permission	= "drwx------";						
						list.Add(item);
					}
				}			
			}
			
			using (DataTable dtFile = ServiceProvider.DAInstance.GetSubFiles(context.UserID, folderID))
			{
				if(dtFile!=null && dtFile.Rows.Count>0)
				{
					DataRow row = null;

					for (int i = 0; i < dtFile.Rows.Count; i++)
					{
						row = dtFile.Rows[i];
						ListItem item = new ListItem();
						item.ItemType = ItemType.File;
						item.LastModifiedTime	= StringUtil.ParseDateTime("" + row["UpdateTime"]);
						item.Length				= StringUtil.ParseLong("" + row["FileSize"]);
						item.Name				= "" + row["FileName"];
						item.Permission			= "-rwx------";
						list.Add(item);
					}
				}
			}
			
			return list;
						
		}

		public override void DeleteFile(FtpContext context, string filename)
		{
			string filePath		= this.CombinePath(context.CurrentPath, filename);
			
			long fileID			= 0;
			string storageDir	= string.Empty;
			long hashID			= 0;
			bool deleteFile		= false;
			
			if(!ServiceProvider.DAInstance.DeleteFile(context.UserID, context.CurrentFolderID, filename,
				out fileID, out hashID, out storageDir, out deleteFile))
			{
				throw new FileNotFoundException(filePath);
			}
			else
			{
				if(deleteFile)
				{
					string storePath = GetStoragePathByID(hashID, storageDir);
					
					if(File.Exists(storePath))
					{
						try
						{
							File.Delete(storePath);
						}
						catch(Exception e)
						{
							LogUtil.LogError(e);
							ServiceProvider.DAInstance.LogDeleteFileFail(hashID, context.UserID, filePath, storePath);
						}					
					}			
				}
			}		
		}		

		public override DateTime GetModifyTime(FtpContext context, string filename)
		{
			string filePath = this.CombinePath(context.CurrentPath, filename);
			
			Model.FileInfo file	= ServiceProvider.DAInstance.GetFileInfoByName(context.UserID, 
				context.CurrentFolderID, filename);
			
			if(file==null)
			{
				throw new FileNotFoundException(filePath);
			}
			return file.UpdateTime;								
		}		

		public override long GetFileSize(FtpContext context, string filename)
		{
			string filePath = this.CombinePath(context.CurrentPath, filename);

			Model.FileInfo file = ServiceProvider.DAInstance.GetFileInfoByName(context.UserID,
				context.CurrentFolderID, filename);

			if (file == null)
			{
				throw new FileNotFoundException(filePath);
			}
			
			return file.FileSize;	
		}		

		public override bool IsExistFolder(FtpContext context, string folderpath, out long folderID)
		{
			folderID = 0;
			
			if(string.IsNullOrEmpty(folderpath))
				return false;

			if (IsRootPath(folderpath))
				return true;
				
			folderID = ServiceProvider.DAInstance.GetFolderIDByPath(context.UserID, folderpath);

			if (folderID > 0)
				return true;
			else
				return false;
				 
		}
		
		public override bool IsExistFile(FtpContext context, string filepath)
		{
			string filename		= string.Empty;
			string parentFolder = FileHelper.GetParentFolder(filepath, out filename);
			long parentID		= 0;
			
			if(string.Compare(parentFolder, context.CurrentPath, StringComparison.OrdinalIgnoreCase)==0)
				parentID = context.CurrentFolderID;
			else
				parentID = ServiceProvider.DAInstance.GetFolderIDByPath(context.UserID, parentFolder);
			
			return (ServiceProvider.DAInstance.GetFileIDByName(context.UserID, parentID, filename)>0);
		}

		public override void CreateFolder(FtpContext context, string foldername)
		{
			ServiceProvider.DAInstance.CreateFolder(context.UserID, context.CurrentFolderID, foldername);	
		}

		public override void RemoveFolder(FtpContext context, string foldername)
		{
			ServiceProvider.DAInstance.DeleteFolder(context.UserID, context.CurrentFolderID, foldername);					
		}

		public override long GetUserRealUsedSpace(string username)
		{
			return ServiceProvider.DAInstance.GetUserUsedSpace(username);
		}

		public override void OnServiceStop()
		{
			
		}

		public override void ReadFile(FtpContext context, string filename, Stream stream)
		{
			string filepath	= this.CombinePath(context.CurrentPath, filename);
			
			string storageDir	= string.Empty;
			
			long fileID, hashID;
			
			if(!ServiceProvider.DAInstance.GetFileStorageInfo(context.UserID, context.CurrentFolderID, filename, out fileID, out hashID, out storageDir))
			{
				throw new FileNotFoundException(filepath);
			}

			base.ReadFile(context, GetStoragePathByID(hashID, storageDir), stream);
		}

		public override void StoreFile(FtpContext context, string filename, Stream stream)
		{
			string filepath = this.CombinePath(context.CurrentPath, filename);

			IHashProvider hashProvider = HashProviderManager.Create();

			
			string storePath	= string.Empty;
			
			
			Model.FileInfo file = null;
			
			if(context.Offset>0)//append
			{			
				file = ServiceProvider.DAInstance.GetFileInfoByName(context.UserID, context.CurrentFolderID, filename);
				
				if(file==null)
					throw new FileNotFoundException(filepath);				
				
				string prevHash		= string.Empty;
				string storageDir	= string.Empty;
				
				if(!ServiceProvider.DAInstance.GetHashStorageInfo(file.HashID, out storageDir, out prevHash))
					throw new FileNotFoundException(filepath);

				storePath = GetStoragePathByID(file.HashID, storageDir);
					
				hashProvider.Setup(file.TempHashCode, file.Offset, file.PrevHashBuffer);
			}
			else//new file
			{
				file	= new Model.FileInfo();
				file.FileName	= filename;
				file.UserID		= context.UserID;
				
				FolderInfo folder	= ServiceProvider.DAInstance.GetFolderInfoByID(context.UserID, context.CurrentFolderID);
				
				if(folder==null)
					throw new DirectoryNotFoundException(context.CurrentPath);
					
				file.ParentID	= folder.FolderID;
				
				storePath	= Path.Combine(context.TempDirectory, Guid.NewGuid().ToString());
			
				hashProvider.Setup();
			}
			
			StoreOption option = new StoreOption();
			option.AppendOriginalFile = true;

			base.StoreFile(context, storePath, stream, option, hashProvider);

			//Save temp hash
			file.SetTempHash(hashProvider.Offset, hashProvider.GetTempHashString(), hashProvider.GetLeftBufferString());
			
			hashProvider.TransformFinalBlock();
			string newHashCode = hashProvider.GetHashString();
			
			long hashID = 0;
			
			if(context.Offset>0)//append
			{
				long oldFileSize = file.FileSize;
				
				file.FileSize += option.TotalRead;
				
				bool deleteOldFile = false;

				string storageDir = string.Empty;

				if (ServiceProvider.DAInstance.AppendNewFile(file, newHashCode, out hashID, out deleteOldFile, out storageDir))
				{
					string newStorePath	= GetStoragePathByID(hashID, storageDir, true);

					//The path is same
					if (string.Compare(newStorePath, storePath, StringComparison.OrdinalIgnoreCase)==0)
						return;
					
					if(deleteOldFile) //move
					{
						if (!File.Exists(newStorePath))
							File.Move(storePath, newStorePath);
					}
					else //Copy
					{
						if (!File.Exists(newStorePath))
							File.Copy(storePath, newStorePath);

						using(FileStream fs = new FileStream(storePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
						{
							//Trim to the old size
							fs.SetLength(oldFileSize);
						}
					}
				}
				else//Update database failed, so restore to original size
				{
					context.ChangeSpace(0-option.TotalRead);

					using (FileStream fs = new FileStream(storePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						//Trim to the old size
						fs.SetLength(oldFileSize);
					}
					
					throw new StoreException();
				}
			}
			else// new uploaded file
			{
				file.FileSize = option.TotalRead;

				string storageDir = string.Empty;

				if (ServiceProvider.DAInstance.UploadNewFile(file, newHashCode, out hashID, out storageDir))
				{
					string newStorePath = GetStoragePathByID(hashID, storageDir, true);
					
					if(!File.Exists(newStorePath))
					{
						File.Move(storePath, newStorePath);
					}					
				}
				else//Insert file into database failed
				{
					context.ChangeSpace(0 - option.TotalRead);
					File.Delete(storePath);//delete the temp file
					throw new StoreException();
				}
			}
		}
		
		private string GetStoragePathByID(long storeID, string dir)
		{
			return GetStoragePathByID(storeID, dir, false);
		}

		private string GetStoragePathByID(long storeID, string dir, bool autoCreateDir)
		{
			string fullDir = string.Empty;

			long node = storeID / 1000;

			fullDir = node.ToString().PadLeft(3, '0');

			node = node / 1000;

			fullDir = Path.Combine(dir, "file\\" + node.ToString().PadLeft(3, '0') + "\\" + fullDir);

			if (autoCreateDir && !Directory.Exists(fullDir))
				Directory.CreateDirectory(fullDir);

			return Path.Combine(fullDir, storeID.ToString());
		}

		private string GetTempStorePath(string sessionID, string dir)
		{
			return GetTempStorePath(sessionID, dir, false);
		}

		private string GetTempStorePath(string sessionID, string dir, bool autoCreate)
		{
			string path = dir + "\\temp\\" + sessionID;			
			
			if (autoCreate && !Directory.Exists(path))
				Directory.CreateDirectory(path);

			return path;
		}
		
		protected string CombinePath(string parent, string current)
		{
			if(string.IsNullOrEmpty(parent))
				return current;
			
			if(!parent.EndsWith("/", StringComparison.OrdinalIgnoreCase))
				parent = parent + "/";
				
			return parent + current;
		}
		
		private bool IsRootPath(string path)
		{
			if(!string.IsNullOrEmpty(path))
			{
				if(path=="/")
					return true;			
			}
			
			return false;
		}

		public override void ClearTempDirectory(FtpContext context)
		{
			if(!string.IsNullOrEmpty(context.TempDirectory))
			{
				try
				{
					Directory.Delete(context.TempDirectory, true);
				}
				catch(Exception e)
				{
					LogUtil.LogError(e);
				}
			}
		}

		public override string GetTempDirectory(string sessionID)
		{
			StorageSetInfo set = ServiceProvider.DAInstance.GetLastStorageSet();

			return GetTempStorePath(sessionID, set.StoragePath, true);
		}

		public override void RenameFile(FtpContext context, string oldPath, string newPath)
		{
			string oldFileName = string.Empty;
			string newFileName = string.Empty;

			long oldParentID = 0;

			string oldParentPath = FileHelper.GetParentFolder(oldPath, out oldFileName);
			string newParentPath = FileHelper.GetParentFolder(newPath, out newFileName);

			if (oldParentPath != newParentPath)
				throw new ArgumentException("Invalid arguments for rename.");

			if (string.Compare(oldParentPath, context.CurrentPath, StringComparison.OrdinalIgnoreCase) == 0)
				oldParentID = context.CurrentFolderID;
			else
				oldParentID = ServiceProvider.DAInstance.GetFolderIDByPath(context.UserID, oldParentPath);

			Model.FileInfo file = ServiceProvider.DAInstance.GetFileInfoByName(context.UserID, oldParentID, oldFileName);

			if (file != null)
			{
				RenameResult result = ServiceProvider.DAInstance.RenameFile(context.UserID, file.FileID, newFileName);
			}
			else
			{
				throw new FileNotFoundException(oldPath);
			}			
		}

		public override void RenameFolder(FtpContext context, string oldPath, string newPath)
		{
			string oldFileName = string.Empty;
			string newFileName = string.Empty;

			long oldParentID = 0;

			string oldParentPath = FileHelper.GetParentFolder(oldPath, out oldFileName);
			string newParentPath = FileHelper.GetParentFolder(newPath, out newFileName);

			if (oldParentPath != newParentPath)
				throw new ArgumentException("Invalid arguments for rename.");

			if (string.Compare(oldParentPath, context.CurrentPath, StringComparison.OrdinalIgnoreCase) == 0)
				oldParentID = context.CurrentFolderID;
			else
				oldParentID = ServiceProvider.DAInstance.GetFolderIDByPath(context.UserID, oldParentPath);

			Model.FolderInfo folder = ServiceProvider.DAInstance.GetFolderInfoByName(context.UserID, oldParentID, oldFileName);

			if (folder != null)
			{
				RenameResult result = ServiceProvider.DAInstance.RenameFolder(context.UserID, folder.FolderID, newFileName);
			}
			else
			{
				throw new DirectoryNotFoundException(oldPath);
			}
		}
	}
}
