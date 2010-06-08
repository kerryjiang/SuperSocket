using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using SuperSocket.SocketServiceCore.Command;
using System.Reflection;
using SuperSocket.Common;
using System.IO;
using SuperSocket.SocketServiceCore.Config;
using System.ServiceModel.Description;

namespace SuperSocket.SocketServiceCore
{
    public interface IAppServer<T> : IRunable<T>, ICommandSource<T>
         where T : IAppSession, new()
    {
        IServerConfig Config { get; }
        T CreateAppSession(ISocketSession socketSession);
    }

    public abstract class AppServer<T> : IAppServer<T> where T : IAppSession, IAppSession<T>, new()
    {
        private IPEndPoint m_LocalEndPoint;

        public IServerConfig Config { get; private set; }

        public AppServer()
		{
            LoadCommands();
		}

        public ServiceCredentials ServerCredentials { get; set; }

        private Dictionary<string, ICommand<T>> dictCommand = new Dictionary<string, ICommand<T>>();
        /// <summary>
        /// Loads the command dictionary.
        /// </summary>
        private void LoadCommands()
        {
            Type commandType = typeof(ICommand<T>);
            Assembly asm = typeof(T).Assembly;
            Type[] arrType = asm.GetExportedTypes();

            for (int i = 0; i < arrType.Length; i++)
            {
                //LogUtil.LogInfo(arrType[i].ToString() + "\r\n");
                Type[] arrInterface = arrType[i].GetInterfaces();

                for (int j = 0; arrInterface != null && j < arrInterface.Length; j++)
                {
                    if (arrInterface[j] == commandType)
                    {
                        //LogUtil.LogInfo(arrInterface[j].ToString() + ":" + commandType.ToString());
                        dictCommand[arrType[i].Name.ToUpper()] = arrType[i].GetConstructor(new Type[0]).Invoke(new object[0]) as ICommand<T>;
                    }
                }
            }
            //LogUtil.LogInfo("Load " + dictCommand.Count + " commands from " + arrType.Length + " types!");		
        }

        

        private Dictionary<string, ProviderBase> m_ProviderDict = new Dictionary<string, ProviderBase>();

        /// <summary>
        /// Setups the specified factory.
        /// </summary>
        /// <param name="assembly">The assembly name.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public virtual bool Setup(string assembly, IServerConfig config)
        {
            Config = config;

            if (string.IsNullOrEmpty(assembly))
                return true;

            if (!string.IsNullOrEmpty(config.Ip) && config.Port > 0)
            {
                try
                {
                    m_LocalEndPoint = new IPEndPoint(IPAddress.Parse(config.Ip), config.Port);
                }
                catch(Exception e)
                {
                    LogUtil.LogError("Invalid config ip/port", e);
                    return false;
                }
            }

            if (m_LocalEndPoint == null)
            {
                LogUtil.LogError("Config ip/port is missing!");
                return false;
            }

            string dir = FileHelper.GetParentFolder(this.GetType().Assembly.Location);

            string assemblyFile = Path.Combine(dir, assembly + ".dll");

            try
            {
                Type typeProvider = typeof(ProviderBase);

                Assembly ass = Assembly.LoadFrom(assemblyFile);

                Type[] arrType = ass.GetTypes();

                for (int i = 0; arrType != null && i < arrType.Length; i++)
                {
                    //Must be a seal class
                    if (arrType[i].IsSubclassOf(typeProvider))
                    {
                        ProviderBase provider = ass.CreateInstance(arrType[i].ToString()) as ProviderBase;
                        if (provider.Init(config))
                        {
                            m_ProviderDict[provider.Name.ToLower()] = provider;
                        }
                        else
                        {
                            LogUtil.LogError("Failed to initalize provider " + arrType[i].ToString() + "!");
                            return false;
                        }
                    }
                }

                if (!IsReady)
                {
                    LogUtil.LogError("Failed to load service provider from assembly:" + assemblyFile);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                return false;
            }
        }

        private ISocketServer m_SocketServer;

        public virtual bool Start()
        {
            if (Config.Mode == SocketMode.Sync)
            {
                m_SocketServer = new SyncSocketServer<SyncSocketSession<T>, T>(this, m_LocalEndPoint);
                if (!m_SocketServer.Start())
                    return false;
            }
            else
            {
                m_SocketServer = new AsyncSocketServer<AsyncSocketSession<T>, T>(this, m_LocalEndPoint);
                if (!m_SocketServer.Start())
                    return false;
            }

            return true;
        }

        public virtual void Stop()
        {
            if (m_SocketServer != null)
            {
                m_SocketServer.Stop();
            }
        }

        /// <summary>
        /// Gets service provider by name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        protected ProviderBase GetProviderByName(string providerName)
        {
            ProviderBase provider = null;

            if (m_ProviderDict.TryGetValue(providerName.ToLower(), out provider))
            {
                return provider;
            }
            else
            {
                return null;
            }
        }        

        public T CreateAppSession(ISocketSession socketSession)
        {
            T appSession = new T();
            appSession.Initialize(this, socketSession);
            return appSession;
        }

        public abstract bool IsReady { get; }

        #region ICommandSource<T> Members

        public ICommand<T> GetCommandByName(string commandName)
        {
            ICommand<T> command;

            if (dictCommand.TryGetValue(commandName, out command))
                return command;
            else
                return null;
        }

        #endregion
    }
}
