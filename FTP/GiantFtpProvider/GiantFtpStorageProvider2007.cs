using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GiantSoft.Common;
using GiantSoft.FtpService;
using GiantFtpProvider.Service;
using GiantFtpProvider.Model;

namespace GiantFtpProvider
{
	public sealed class GiantFtpStorageProvider2007 : GiantFtpStorageProvider
	{
		private Dictionary<string, string> m_SystemDirDict = null;		
		private DateTime m_SystemDirDictExpireTime = DateTime.Now;		
		private object m_SyncRoot = new object();
		
		private bool IsSystemDir(string path)
		{
			if (m_SystemDirDict == null || DateTime.Now > m_SystemDirDictExpireTime)
			{
				lock(m_SyncRoot)
				{
					m_SystemDirDict = new Dictionary<string,string>();
					
					List<SystemFolderInfo> systemFoders = ServiceProvider.DAInstance.GetSystemFolders();
					
					foreach(SystemFolderInfo folder in systemFoders)
					{
						m_SystemDirDict[folder.FolderPath.ToLower()] = folder.FolderPath;
					}
					
					m_SystemDirDictExpireTime = DateTime.Now.AddHours(1);
				}
			}
			return m_SystemDirDict.ContainsKey(path.ToLower());
		}

		public override void RemoveFolder(FtpContext context, string foldername)
		{
			if(IsSystemDir(CombinePath(context.CurrentPath, foldername)))
			{
				throw new SystemDirectoryException();
			}			
			base.RemoveFolder(context, foldername);
		}

		public override void RenameFolder(FtpContext context, string oldPath, string newPath)
		{
			if(IsSystemDir(oldPath))
			{
				throw new SystemDirectoryException();
			}			
			base.RenameFolder(context, oldPath, newPath);
		}
	}
}
