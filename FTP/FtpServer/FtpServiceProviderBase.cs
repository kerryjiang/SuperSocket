using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.FtpService.Storage;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.FtpService
{
	public abstract class FtpServiceProviderBase : ProviderBase
	{
        protected object SyncRoot = new object();

		public override string Name
		{
			get { return "FtpServiceProvider"; }
		}

        public override bool Init(IServerConfig config)
        {
            base.Init(config);

            NameValueConfigurationElement dataPortNode = config.Parameters["dataPort"];

            if (dataPortNode == null)
            {
                LogUtil.LogError("Parameter 'dataPort' is required!");
                return false;
            }

            string[] range = dataPortNode.Value.Split(',', ';', ':');

            for (int i = 0; i < range.Length; i++)
            {
                string portField = range[i];

                if (string.IsNullOrEmpty(portField))
                    continue;

                string[] arrPorts = portField.Split('-');

                if (arrPorts == null || arrPorts.Length < 1 || arrPorts.Length > 2)
                {
                    LogUtil.LogError("Invalid 'dataPort' value in parameter!");
                    return false;
                }

                if (arrPorts.Length == 1)
                {
                    int port;

                    if (!int.TryParse(arrPorts[0], out port))
                    {
                        LogUtil.LogError("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    m_DataPorts.Add(port);
                }
                else if (arrPorts.Length == 2)
                {
                    int portStart, portEnd;

                    if (!int.TryParse(arrPorts[0], out portStart))
                    {
                        LogUtil.LogError("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    if (!int.TryParse(arrPorts[1], out portEnd))
                    {
                        LogUtil.LogError("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    if (portEnd <= portStart)
                    {
                        LogUtil.LogError("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    if (portStart < 0)
                        portStart = 1;

                    if (portEnd > 65535)
                        portEnd = 65535;

                    for (int seq = portStart; seq < portEnd + 1; seq++)
                    {
                        m_DataPorts.Add(seq);
                    }
                }
            }

            return true;
        }

		public override void OnServiceStop()
		{
			
		}

        private List<int> m_DataPorts = new List<int>();

        private Random m_PortIndexCreator = new Random();

        public virtual int GetRandomPort()
        {
            lock (SyncRoot)
            {
                return m_DataPorts[m_PortIndexCreator.Next(0, m_DataPorts.Count - 1)];
            }
        }

		/// <summary>
		/// Authenticates the specified username and password.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public abstract AuthenticationResult Authenticate(string username, string password, out FtpUser user);

		/// <summary>
		/// Updates the used space for user.
		/// </summary>
		/// <param name="user">The user.</param>
		public abstract void UpdateUsedSpaceForUser(FtpUser user);

		/// <summary>
		/// Gets all users.
		/// </summary>
		/// <returns></returns>
		public abstract List<FtpUser> GetAllUsers();

		/// <summary>
		/// Gets the FTP user by ID.
		/// </summary>
		/// <param name="userID">The user ID.</param>
		/// <returns></returns>
		public abstract FtpUser GetFtpUserByID(long userID);

		/// <summary>
		/// Gets the FTP user by username.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <returns></returns>
		public abstract FtpUser GetFtpUserByUserName(string username);


		/// <summary>
		/// Updates the FTP user.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public abstract bool UpdateFtpUser(FtpUser user);

		/// <summary>
		/// Creates the FTP user.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <returns></returns>
		public abstract CreateUserResult CreateFtpUser(FtpUser user);


		/// <summary>
		/// Changes the user's password.
		/// </summary>
		/// <param name="userID">The user ID.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		public abstract bool ChangePassword(long userID, string password);


		public virtual List<ListItem> GetList(FtpContext context)
		{
			string dir = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/'));

			return GetList(context, dir);
		}

		protected List<ListItem> GetList(FtpContext context, string dir)
		{
			List<ListItem> list = new List<ListItem>();

			if (Directory.Exists(dir))
			{
				string[] files = Directory.GetFiles(dir);

				if (files != null)
				{
					for (int i = 0; i < files.Length; i++)
					{
						FileInfo file = new FileInfo(files[i]);
						ListItem item = new ListItem();

						item.ItemType = ItemType.File;
						item.LastModifiedTime = file.LastWriteTime;
						item.Length = file.Length;
						item.Name = file.Name;
						item.OwnerName = context.UserName;
						item.Permission = "-rwx------";
						list.Add(item);
					}
				}

				string[] folders = Directory.GetDirectories(dir);

				if (folders != null)
				{
					for (int i = 0; i < folders.Length; i++)
					{
						DirectoryInfo folder = new DirectoryInfo(folders[i]);
						ListItem item = new ListItem();

						item.ItemType = ItemType.Folder;
						item.LastModifiedTime = folder.LastWriteTime;
						item.Length = 0;
						item.Name = folder.Name;
						item.OwnerName = context.UserName;
						item.Permission = "drwx------";

						list.Add(item);
					}
				}

				return list;
			}
			else
			{
				context.SetError(Resource.GetString("NotFound_550"));
				return null;
			}
		}

		public virtual bool DeleteFile(FtpContext context, string filename)
		{
			string filepath = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + filename);

			if (File.Exists(filepath))
			{
				FileInfo file = new FileInfo(filepath);

				long fileLength = file.Length;

				try
				{
					File.Delete(filepath);
					context.ChangeSpace(0 - fileLength);
					return true;
				}
				catch (UnauthorizedAccessException uae)
				{
					LogUtil.LogError(uae);
					context.SetError(Resource.GetString("PermissionDenied_550"));
					return false;
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
					context.SetError(Resource.GetString("DeleteFailed_450"), filename);
					return false;
				}
			}
			else
			{
				context.SetError(Resource.GetString("NotFound_550"));
				return false;
			}
		}

		public virtual DateTime GetModifyTime(FtpContext context, string filename)
		{
			string filepath = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + filename);

			if (File.Exists(filepath))
			{
				try
				{
					FileInfo file = new FileInfo(filepath);
					return file.LastWriteTime;
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
					context.SetError(Resource.GetString("FileUnavailable_550"));
					return DateTime.MinValue;
				}
			}
			else
			{
				context.SetError(Resource.GetString("NotFound_550"));
				return DateTime.MinValue;
			}
		}

		public virtual long GetFileSize(FtpContext context, string filename)
		{
			string filepath = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + filename);

			if (File.Exists(filepath))
			{
				try
				{
					FileInfo file = new FileInfo(filepath);
					return file.Length;
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
					context.SetError(Resource.GetString("FileUnavailable_550"));
					return 0;
				}
			}
			else
			{
				context.SetError(Resource.GetString("NotFound_550"));
				return 0;
			}
		}

		public virtual bool RenameFile(FtpContext context, string oldPath, string newPath)
		{
			oldPath = CombinePath(context.User.Root, oldPath);
			newPath = CombinePath(context.User.Root, newPath);

			try
			{
				File.Move(oldPath, newPath);
				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				context.SetError(Resource.GetString("RenameToFailed_553"));
				return false;
			}
		}

		public virtual bool RenameFolder(FtpContext context, string oldPath, string newPath)
		{
			oldPath = CombinePath(context.User.Root, oldPath);
			newPath = CombinePath(context.User.Root, newPath);

			try
			{
				Directory.Move(oldPath, newPath);
				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				context.SetError(Resource.GetString("RenameDirectoryFailed_553"));
				return false;
			}
		}

		public virtual bool IsExistFolder(FtpContext context, string path, out long folderID)
		{
			folderID = (long)path.GetHashCode();

			string dir = CombinePath(context.User.Root, StringUtil.ReverseSlash(path, '/'));

			try
			{
				return Directory.Exists(dir);
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				context.SetError(Resource.GetString("FileSystemError_450"));
				return false;
			}
		}

		public virtual bool IsExistFile(FtpContext context, string filepath)
		{
			string path = CombinePath(context.User.Root, StringUtil.ReverseSlash(filepath, '/'));

			try
			{
				return File.Exists(path);
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				context.SetError(Resource.GetString("FileSystemError_450"));
				return false;
			}
		}

		public virtual bool StoreFile(FtpContext context, string filename, Stream stream)
		{
			return StoreFile(context, filename, stream, new StoreOption());
		}

		protected bool StoreFile(FtpContext context, string filename, Stream stream, StoreOption option)
		{
			int bufLen = 1024 * 10;
			byte[] buffer = new byte[bufLen];
			int read = 0;
			long totalRead = 0;

			int speed = context.User.MaxUploadSpeed;

			FileStream fs = null;

			try
			{
				if (context.Offset > 0 && option.AppendOriginalFile) //Append
				{
					FileInfo file = new FileInfo(filename);

					if (context.Offset != file.Length)
					{
						context.Status = SocketContextStatus.Error;
						context.Message = "Invalid offset";
						return false;
					}

					fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Write, bufLen);
				}
				else
				{
					if (File.Exists(filename))
					{
						FileInfo file = new FileInfo(filename);
						File.Delete(filename);
						context.ChangeSpace(0 - file.Length);
					}

					fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.Write, bufLen);
				}


				DateTime dtStart;
				TimeSpan ts;
				int usedMs = 0, predictMs = 0;

				dtStart = DateTime.Now;

				while ((read = stream.Read(buffer, 0, bufLen)) > 0)
				{
					fs.Write(buffer, 0, read);
					totalRead += read;
					context.ChangeSpace(read);

					if (speed > 0) // if speed <=0, then no speed limitation
					{
						ts = DateTime.Now.Subtract(dtStart);
						usedMs = (int)ts.TotalMilliseconds;
						predictMs = read / speed;

						if (predictMs > usedMs) //Speed control
						{
							Thread.Sleep(predictMs - usedMs);
						}

						dtStart = DateTime.Now;
					}
				}

				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				context.Status = SocketContextStatus.Error;
				context.Message = "Store file error";
				return false;
			}
			finally
			{
				option.TotalRead = totalRead;

				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
					fs = null;
				}
			}
		}

		public virtual bool CreateFolder(FtpContext context, string foldername)
		{
			string dir = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + foldername);

			if (Directory.Exists(dir) || File.Exists(dir))
			{
				context.SetError(Resource.GetString("DirectoryAlreadyExist_550"), foldername);
				return false;
			}
			else
			{
				try
				{
					Directory.CreateDirectory(dir);
					return true;
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
					context.SetError(Resource.GetString("MakeDirFailed_550"), foldername);
					return false;
				}
			}
		}

		public virtual bool RemoveFolder(FtpContext context, string foldername)
		{
			string dir = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + foldername);

			if (Directory.Exists(dir))
			{
				try
				{
					Directory.Delete(dir);
					return true;
				}
				catch (Exception e)
				{
					LogUtil.LogError(e);
					context.SetError(Resource.GetString("RemoveDirectoryFailed_550"), foldername);
					return false;
				}
			}
			else
			{
				context.SetError(Resource.GetString("NotFound_550"));
				return false;
			}
		}

		public virtual bool ReadFile(FtpContext context, string filename, Stream stream)
		{
			int bufLen = 1024 * 10;
			byte[] buffer = new byte[bufLen];
			int read = 0;

			FileStream fs = null;

			try
			{
				fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufLen);

				if (context.Offset > 0)
					fs.Seek(context.Offset, SeekOrigin.Begin);

				DateTime dtStart;
				TimeSpan ts;
				int usedMs = 0, predictMs = 0;
				int speed = context.User.MaxDownloadSpeed;

				dtStart = DateTime.Now;

				while ((read = fs.Read(buffer, 0, bufLen)) > 0)
				{
					stream.Write(buffer, 0, read);

					if (speed > 0) // if speed <=0, then no speed limitation
					{
						ts = DateTime.Now.Subtract(dtStart);
						usedMs = (int)ts.TotalMilliseconds;
						predictMs = read / speed;

						if (predictMs > usedMs) //Speed control
							Thread.Sleep(predictMs - usedMs);

						dtStart = DateTime.Now;
					}
				}

				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				context.SetError("Failed to delete file");
				return false;
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
					fs = null;
				}
			}

		}

		public abstract long GetUserRealUsedSpace(FtpContext context, string username);

		public abstract string GetTempDirectory(string sessionID);

		public abstract void ClearTempDirectory(FtpContext context);

		protected string CombinePath(string root, string relativePath)
		{
			if (root.EndsWith("\\", StringComparison.Ordinal))
			{
				root = root.Substring(0, root.Length - 1);
			}

			if (!relativePath.StartsWith("\\", StringComparison.Ordinal))
			{
				relativePath = "\\" + relativePath;
			}

			return root + relativePath;
		}

		protected long GetDirectorySize(string dir)
		{
			long total = 0;

			string[] arrFiles = Directory.GetFiles(dir);

			if (arrFiles != null && arrFiles.Length > 0)
			{
				for (int i = 0; i < arrFiles.Length; i++)
				{
					FileInfo file = new FileInfo(arrFiles[i]);
					total += file.Length;
				}
			}

			string[] arrDirs = Directory.GetDirectories(dir);

			if (arrDirs != null && arrDirs.Length > 0)
			{
				for (int i = 0; i < arrDirs.Length; i++)
				{
					total += GetDirectorySize(arrDirs[i]);
				}
			}

			return total;
		}
	}
}
