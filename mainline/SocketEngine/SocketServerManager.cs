using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketEngine
{
    public static class SocketServerManager
    {
        private static List<IAppServer> m_ServerList = new List<IAppServer>();

        private static Dictionary<string, Type> m_ServiceDict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        private static IConfig m_Config;

        /// <summary>
        /// Initializes with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static bool Initialize(IConfig config)
        {
            m_Config = config;

            //Initialize services
            List<IServiceConfig> serviceList = config.GetServiceList();

            Type serviceType;

            foreach (var service in serviceList)
            {
                if (service.Disabled)
                    continue;

                if (!AssemblyUtil.TryGetType(service.Type, out serviceType))
                {
                    LogUtil.LogError("Failed to initialize service " + service.ServiceName + "!");
                    return false;
                }

                m_ServiceDict[service.ServiceName] = serviceType;
            }

            //Initialize servers
            foreach (var serverConfig in config.GetServerList())
            {
                if (!InitializeServer(serverConfig))
                {
                    LogUtil.LogError("Failed to initialize server " + serverConfig.Name + "!");
                }
            }

            return true;
        }

        private static bool InitializeServer(IServerConfig serverConfig)
        {
            if (serverConfig.Disabled)
                return true;

            Type serviceType = null;

            if (!m_ServiceDict.TryGetValue(serverConfig.ServiceName, out serviceType))
            {
                LogUtil.LogError(string.Format("The service {0} cannot be found in configuration!", serverConfig.ServiceName));
                return false;
            }

            IAppServer server;

            try
            {
                server = (IAppServer)Activator.CreateInstance(serviceType);
            }
            catch (Exception e)
            {
                LogUtil.LogError("Failed to create server instance!", e);
                return false;
            }

            if (!server.Setup(m_Config, serverConfig, SocketServerFactory.Instance, GetServiceProvider(serverConfig.ServiceName, serverConfig.Provider)))
            {
                LogUtil.LogError("Failed to setup server instance!");
                return false;
            }

            m_ServerList.Add(server);
            return true;
        }

        public static bool Start()
        {
            foreach (IAppServer server in m_ServerList)
            {
                if (!server.Start())
                {
                    LogUtil.LogError("Failed to start " + server.Name + " server!");
                }
                else
                {
                    LogUtil.LogInfo(server.Name + " has been started");
                }
            }

            return true;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public static void Stop()
        {
            foreach (var server in m_ServerList)
            {
                server.Stop();
                LogUtil.LogInfo(server.Name + " has been stopped");
            }
        }

        public static IServiceConfig GetServiceConfig(string name)
        {
            foreach (var config in m_Config.GetServiceList())
            {
                if (string.Compare(config.ServiceName, name, true) == 0)
                {
                    return config;
                }
            }
            return null;
        }

        public static string GetServiceProvider(string service, string provider)
        {
            IServiceConfig config = GetServiceConfig(service);

            if (config == null)
                return string.Empty;

            NameValueConfigurationElement element = config.Providers[provider];

            if (element == null)
                return string.Empty;
            else
                return element.Value;
        }

        public static IAppServer GetServerByName(string name)
        {
            return m_ServerList.SingleOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        //public static ServiceCredentials GetServiceCredentials(ICredentialConfig config)
        //{
        //    ServiceCredentials credential = new ServiceCredentials();

        //    try
        //    {
        //        credential.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
        //        credential.UserNameAuthentication.CachedLogonTokenLifetime = TimeSpan.FromHours(1);
        //        credential.UserNameAuthentication.CacheLogonTokens = true;
        //        credential.UserNameAuthentication.CustomUserNamePasswordValidator = new SocketManagerPasswordValidator(config);
        //        credential.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, "GiantSocketServer");
        //    }
        //    catch (Exception e)
        //    {
        //        //X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        //        //store.Open(OpenFlags.ReadWrite);
        //        //LogUtil.LogInfo("All certificates count:" + store.Certificates.Count);
        //        //X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
        //        //LogUtil.LogInfo("Certificates count:" + certs.Count);
        //        //if (certs != null && certs.Count > 1)
        //        //{
        //        //    store.Remove(certs[0]);
        //        //    store.Close();
        //        //}
        //        LogUtil.LogError(e);
        //        credential = null;
        //    }

        //    return credential;
        //}
    }
}
