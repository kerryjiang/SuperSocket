using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.FtpService;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Config;
using System.Configuration;
using SuperSocket.Common;

namespace SuperSocket.XmlConfigFTP
{
    public class XmlFtpProvider : FtpServiceProviderBase
    {
        private string m_UserSettingFile;

        private Dictionary<string, FtpUser> m_UserDict = new Dictionary<string, FtpUser>(StringComparer.OrdinalIgnoreCase);
        
        public override bool Init(IServerConfig config)
        {
            if (!base.Init(config))
                return false;

            NameValueConfigurationElement userSettingElement = config.Parameters["userSetting"];

            if (userSettingElement == null)
            {
                LogUtil.LogError("Parameter 'userSetting' is required!");
                return false;
            }

            m_UserSettingFile = userSettingElement.Value;

            List<FtpUser> users;

            if (!XmlSerializerUtil.TryDeserialize<List<FtpUser>>(m_UserSettingFile, out users))
            {
                LogUtil.LogError("Invalid userSetting file!");

                users = new List<FtpUser>();
                users.Add(new FtpUser
                    {
                        UserName = "anonymous",
                        Password = "*",
                        Root = @"D:\",
                        MaxThread = 5
                    });
                users.Add(new FtpUser
                {
                    UserName = "kerry",
                    Password = "123456",
                    Root = @"D:\",
                    MaxThread = 5
                });

                XmlSerializerUtil.Serialize(m_UserSettingFile, users);

                return false;
            }

            foreach (var u in users)
            {
                m_UserDict[u.UserName] = u;
            }

            return true;
        }

        public override AuthenticationResult Authenticate(string username, string password, out FtpUser user)
        {
            lock (SyncRoot)
            {
                if (!m_UserDict.TryGetValue(username, out user))
                    return AuthenticationResult.NotExist;

                if ("*".Equals(user.Password))
                    return AuthenticationResult.Success;

                if (user.Password.Equals(user.Password, StringComparison.OrdinalIgnoreCase))
                    return AuthenticationResult.PasswordError;

                return AuthenticationResult.Success;
            }
        }

        public override void UpdateUsedSpaceForUser(FtpUser user)
        {
            //Do nothing
        }

        public override List<FtpUser> GetAllUsers()
        {
            lock (SyncRoot)
            {
                return m_UserDict.Values.ToList();
            }
        }

        public override FtpUser GetFtpUserByID(long userID)
        {
            throw new NotImplementedException();
        }

        public override FtpUser GetFtpUserByUserName(string username)
        {
            lock (SyncRoot)
            {
                FtpUser user;
                m_UserDict.TryGetValue(username, out user);
                return user;
            }
        }

        public override bool UpdateFtpUser(FtpUser user)
        {
            //Do nothing
            return true;
        }

        public override CreateUserResult CreateFtpUser(FtpUser user)
        {
            //Do nothing
            return CreateUserResult.UnknownError;
        }

        public override bool ChangePassword(long userID, string password)
        {
            throw new NotImplementedException();
        }

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
            //Do nothing
        }
    }
}
