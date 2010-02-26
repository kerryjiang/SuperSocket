using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using GiantSoft.Common;
using GiantSoft.FtpService;
using GiantSoft.SocketServiceCore;
using GiantSoft.SocketServiceCore.Config;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace GiantSoft.SimpleFtpProvider
{
	public class SimpleFtpServiceProvider : FtpServiceProviderBase
	{
		private Dictionary<string, FtpUser> dictUsers = new Dictionary<string, FtpUser>();

		private GenericDatabase m_Database;

		private List<int> m_DataPorts = new List<int>();

		private Random m_PortIndexCreator = new Random();

		private object m_SyncRoot = new object();

		/// <summary>
		/// Inits the specified config.
		/// </summary>
		/// <param name="config">The config.</param>
		/// <returns></returns>
		public override bool Init(IServerConfig config)
		{
			base.Init(config);

			NameValueConfigurationElement connNode = config.Parameters["ConnectionString"];

			if (connNode == null)
			{
				LogUtil.LogError("The parameter 'ConnectionString' is empty in config!");
				return false;
			}

			m_Database = new GenericDatabase(connNode.Value, OleDbFactory.Instance);

			DbCommand cmd = m_Database.GetSqlStringCommand("select count(*) from FtpUsers");

			try
			{
				int userCount = (int)m_Database.ExecuteScalar(cmd);
				LogUtil.LogInfo(string.Format("{0} user loaded", userCount));
			}
			catch (Exception e)
			{
				LogUtil.LogError("The 'ConnectionString' is not configured correctly!");
				LogUtil.LogError(e);
				return false;
			}

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

				string[] arrPorts = portField.Split('=');

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


		public override AuthenticationResult Authenticate(string username, string password, out FtpUser user)
		{
			user = GetFtpUserByUserName(username);

			if (user == null)
			{
				return AuthenticationResult.NotExist;
			}

			if (string.Compare(user.Password, password, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return AuthenticationResult.Success;
			}
			else
			{
				return AuthenticationResult.PasswordError;
			}
		}

		public override void UpdateUsedSpaceForUser(FtpUser user)
		{
			string updateSql = "update FtpUsers set UsedSpace=@UsedSpace where UserID=@UserID";

			DbCommand cmd = m_Database.GetSqlStringCommand(updateSql);

			m_Database.AddInParameter(cmd, "@UsedSpace", DbType.Int64, user.UsedSpace);

			try
			{
				m_Database.ExecuteNonQuery(cmd);
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
			}
		}


		private FtpUser GetFtpUserFromDataRow(DataRow row)
		{
			FtpUser user = new FtpUser();

			user.UserID = StringUtil.ParseLong(row["UserID"].ToString());
			user.UserName = row["UserName"].ToString();
			user.UsedSpace = StringUtil.ParseLong(row["UsedSpace"].ToString());
			user.Root = row["StorageRoot"].ToString();
			user.Password = row["Password"].ToString();
			user.MaxUploadSpeed = StringUtil.ParseInt(row["MaxUploadSpeed"].ToString());
			user.MaxDownloadSpeed = StringUtil.ParseInt(row["MaxDownloadSpeed"].ToString());
			user.MaxThread = StringUtil.ParseInt(row["MaxThread"].ToString());
			user.MaxSpace = StringUtil.ParseLong(row["MaxSpace"].ToString());
			user.LoginTimes = StringUtil.ParseInt(row["LoginTimes"].ToString());
			user.LastLoginTime = StringUtil.ParseDateTime(row["LastLoginTime"].ToString());
			user.Disabled = StringUtil.ParseBool(row["Disabled"].ToString());
			user.CreateTime = StringUtil.ParseDateTime(row["CreateTime"].ToString());

			return user;
		}

		public override List<FtpUser> GetAllUsers()
		{
			List<FtpUser> users = new List<FtpUser>();

			DbCommand cmd = m_Database.GetSqlStringCommand("select * from FtpUsers");

			try
			{
				DataSet resultSet = m_Database.ExecuteDataSet(cmd);
				if (resultSet.Tables.Count > 0)
				{
					DataTable resultTable = resultSet.Tables[0];

					for (int i = 0; i < resultTable.Rows.Count; i++)
					{
						users.Add(GetFtpUserFromDataRow(resultTable.Rows[i]));
					}
				}
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
			}

			return users;
		}

		public override FtpUser GetFtpUserByID(long userID)
		{
			DbCommand cmd = m_Database.GetSqlStringCommand("select * from FtpUsers where UserID=@UserID");
			m_Database.AddInParameter(cmd, "@UserID", DbType.Int64, userID);

			try
			{
				DataSet resultSet = m_Database.ExecuteDataSet(cmd);

				if (resultSet.Tables.Count > 0 && resultSet.Tables[0].Rows.Count > 0)
				{
					return GetFtpUserFromDataRow(resultSet.Tables[0].Rows[0]);
				}
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
			}

			return null;
		}

		public override FtpUser GetFtpUserByUserName(string username)
		{
			DbCommand cmd = m_Database.GetSqlStringCommand("select * from FtpUsers where UserName=@UserName");
			m_Database.AddInParameter(cmd, "@UserName", DbType.String, username);

			try
			{
				DataSet resultSet = m_Database.ExecuteDataSet(cmd);

				if (resultSet.Tables.Count > 0 && resultSet.Tables[0].Rows.Count > 0)
				{
					return GetFtpUserFromDataRow(resultSet.Tables[0].Rows[0]);
				}
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
			}

			return null;
		}

		public override bool UpdateFtpUser(FtpUser user)
		{
			string updateSql = "update FtpUsers set StorageRoot=@StorageRoot, Password=@Password, MaxUploadSpeed=@MaxUploadSpeed,";
			updateSql += " MaxDownloadSpeed=@MaxDownloadSpeed, MaxThread=@MaxThread, MaxSpace=@MaxSpace, Disabled=@Disabled";
			updateSql += " where UserID=@UserID";

			DbCommand cmd = m_Database.GetSqlStringCommand(updateSql);

			m_Database.AddInParameter(cmd, "@StorageRoot", DbType.String, user.Root);
			m_Database.AddInParameter(cmd, "@Password", DbType.String, user.Password);
			m_Database.AddInParameter(cmd, "@MaxUploadSpeed", DbType.Int32, user.MaxUploadSpeed);
			m_Database.AddInParameter(cmd, "@MaxDownloadSpeed", DbType.Int32, user.MaxDownloadSpeed);
			m_Database.AddInParameter(cmd, "@MaxThread", DbType.Int32, user.MaxThread);
			m_Database.AddInParameter(cmd, "@MaxSpace", DbType.Int64, user.MaxSpace);
			m_Database.AddInParameter(cmd, "@Disabled", DbType.Boolean, user.Disabled);

			try
			{
				if (m_Database.ExecuteNonQuery(cmd) == 1)
					return true;
				else
					return false;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
			}

			return false;
		}

		public override CreateUserResult CreateFtpUser(FtpUser user)
		{
			string updateSql = "insert into FtpUsers([UserName], [Password], MaxSpace, UsedSpace, MaxThread, MaxUploadSpeed, MaxDownloadSpeed, StorageRoot)";
			updateSql += " values(@UserName, @Password, @MaxSpace, @UsedSpace, @MaxThread, @MaxUploadSpeed, @MaxDownloadSpeed, @StorageRoot)";
			DbCommand cmd = m_Database.GetSqlStringCommand(updateSql);

			m_Database.AddInParameter(cmd, "@UserName", DbType.String, user.UserName);
			m_Database.AddInParameter(cmd, "@Password", DbType.String, user.Password);
			m_Database.AddInParameter(cmd, "@MaxSpace", DbType.Int64, user.MaxSpace);
			m_Database.AddInParameter(cmd, "@UsedSpace", DbType.Int64, user.UsedSpace);
			m_Database.AddInParameter(cmd, "@MaxThread", DbType.Int32, user.MaxThread);
			m_Database.AddInParameter(cmd, "@MaxUploadSpeed", DbType.Int32, user.MaxUploadSpeed);
			m_Database.AddInParameter(cmd, "@MaxDownloadSpeed", DbType.Int32, user.MaxDownloadSpeed);
			m_Database.AddInParameter(cmd, "@StorageRoot", DbType.String, user.Root);

			try
			{
				if (m_Database.ExecuteNonQuery(cmd) == 1)
					return CreateUserResult.Success;
				else
					return CreateUserResult.UnknownError;
			}
			catch (ConstraintException)
			{
				return CreateUserResult.UserNameAlreadyExist;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				return CreateUserResult.UnknownError;
			}
		}

		public override bool ChangePassword(long userID, string password)
		{
			string updateSql = "update FtpUsers set Password=@Password where UserID=@UserID";

			DbCommand cmd = m_Database.GetSqlStringCommand(updateSql);

			m_Database.AddInParameter(cmd, "@Password", DbType.String, password);
			m_Database.AddInParameter(cmd, "@UserID", DbType.Int64, userID);

			try
			{
				if (m_Database.ExecuteNonQuery(cmd) == 1)
					return true;
				else
					return false;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
			}

			return false;
		}

		/// <summary>
		/// Store the file to the server.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="filename">The filename.</param>
		/// <param name="stream">The socket stream.</param>
		public override bool StoreFile(FtpContext context, string filename, Stream stream)
		{
			string filepath = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + filename);

			return base.StoreFile(context, filepath, stream);
		}


		public override bool ReadFile(FtpContext context, string filename, Stream stream)
		{
			string filepath = CombinePath(context.User.Root, StringUtil.ReverseSlash(context.CurrentPath, '/') + "\\" + filename);

			return base.ReadFile(context, filepath, stream);
		}

		public override void OnServiceStop()
		{

		}

		/// <summary>
		/// Gets the user real used space.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public override long GetUserRealUsedSpace(FtpContext context, string username)
		{
			string userStorageRoot = context.User.Root;

			return GetDirectorySize(userStorageRoot);
		}

		public override string GetTempDirectory(string sessionID)
		{
			return string.Empty;
		}

		public override void ClearTempDirectory(FtpContext context)
		{

		}

		public override int GetRandomPort()
		{
			lock (m_SyncRoot)
			{
				return m_DataPorts[m_PortIndexCreator.Next(0, m_DataPorts.Count - 1)];
			}
		}
	}
}
