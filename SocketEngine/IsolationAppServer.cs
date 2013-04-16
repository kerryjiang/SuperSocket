using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;
using SuperSocket.SocketBase.Metadata;
using System.IO;
using System.Reflection;

namespace SuperSocket.SocketEngine
{
    abstract class IsolationAppServer : MarshalByRefObject, IWorkItem, IStatusInfoSource, IDisposable
    {
        protected const string WorkingDir = "InstancesRoot";

        protected string ServerTypeName { get; private set; }

        protected IBootstrap Bootstrap { get; private set; }

        protected IServerConfig ServerConfig { get; private set; }

        protected ProviderFactoryInfo[] Factories { get; private set; }

        protected IWorkItemBase AppServer { get; private set; }

        public string Name { get; private set; }

        private StatusInfoAttribute[] m_ServerStatusMetadata;

        protected IsolationAppServer(string serverTypeName)
        {
            State = ServerState.NotInitialized;
            ServerTypeName = serverTypeName;
        }

        protected AppDomain CreateHostAppDomain()
        {
            var currentDomain = AppDomain.CurrentDomain;

            var workingDir = Path.Combine(Path.Combine(currentDomain.BaseDirectory, WorkingDir), Name);

            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);

            var startupConfigFile = Bootstrap.StartupConfigFile;

            if (!string.IsNullOrEmpty(startupConfigFile))
            {
                if (!Path.IsPathRooted(startupConfigFile))
                    startupConfigFile = Path.Combine(currentDomain.BaseDirectory, startupConfigFile);
            }

            var hostAppDomain = AppDomain.CreateDomain(Name, currentDomain.Evidence, new AppDomainSetup
            {
                ApplicationName = Name,
                ApplicationBase = workingDir,
                ConfigurationFile = startupConfigFile
            });

            var assemblyImportType = typeof(AssemblyImport);

            hostAppDomain.CreateInstanceFrom(assemblyImportType.Assembly.CodeBase,
                    assemblyImportType.FullName,
                    true,
                    BindingFlags.CreateInstance,
                    null,
                    new object[] { currentDomain.BaseDirectory },
                    null,
                    new object[0]);

            return hostAppDomain;
        }

        public virtual bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories)
        {
            State = ServerState.Initializing;
            Name = config.Name;
            Bootstrap = bootstrap;
            ServerConfig = config;
            Factories = factories;

            if (!bootstrap.Config.DisablePerformanceDataCollector)
            {
                if (!LoadServerStatusMetadata())
                    return false;
            }

            State = ServerState.NotStarted;
            return true;
        }

        protected virtual StatusInfoAttribute[] PrepareServerStatusMetadata(StatusInfoAttribute[] rawMetadata)
        {
            return rawMetadata;
        }

        private bool LoadServerStatusMetadata()
        {
            AppDomain setupAppDomain = null;

            try
            {
                setupAppDomain = CreateHostAppDomain();

                const string statusInfoAttsKey = "statusInfoAtts";

                setupAppDomain.DoCallBack(new CrossAppDomainDelegate(() =>
                    {
                        var serverType = Type.GetType(ServerTypeName);
                        AppDomain.CurrentDomain.SetData(statusInfoAttsKey, MetadataExtensions.GetAllStatusInfoAtttributes(serverType));
                    }));

                m_ServerStatusMetadata = PrepareServerStatusMetadata(setupAppDomain.GetData(statusInfoAttsKey) as StatusInfoAttribute[]);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (setupAppDomain != null)
                {
                    AppDomain.Unload(setupAppDomain);
                }
            }
        }

        protected abstract IWorkItemBase Start();

        bool IWorkItemBase.Start()
        {
            State = ServerState.Starting;

            AppServer = Start();

            if (AppServer != null)
            {
                State = ServerState.Running;
                return true;
            }
            else
            {
                State = ServerState.NotStarted;
                return false;
            }
        }

        public ServerState State { get; private set; }

        protected abstract void Stop();

        void IWorkItemBase.Stop()
        {
            var appServer = AppServer;

            if (appServer == null)
                return;

            State = ServerState.Stopping;
            appServer.Stop();
            Stop();
        }

        protected virtual void OnStopped()
        {
            State = ServerState.NotStarted;
            AppServer = null;
        }

        public int SessionCount
        {
            get
            {
                var appServer = AppServer;

                if (appServer == null)
                    return 0;

                return appServer.SessionCount;
            }
        }

        public StatusInfoAttribute[] GetServerStatusMetadata()
        {
            return m_ServerStatusMetadata;
        }

        private StatusInfoCollection m_PrevStatus;
        private StatusInfoCollection m_StoppedStatus;

        private StatusInfoCollection GetStoppedStatus()
        {
            if (m_StoppedStatus != null)
            {
                m_StoppedStatus = new StatusInfoCollection();
                m_StoppedStatus.Name = Name;
                m_StoppedStatus.Tag = Name;
                m_StoppedStatus[ServerStatusInfoMetadata.IsRunning] = false;
                m_StoppedStatus[ServerStatusInfoMetadata.MaxConnectionNumber] = ServerConfig.MaxConnectionNumber;

                if (m_PrevStatus != null)
                {
                    m_StoppedStatus[ServerStatusInfoMetadata.Listeners] = m_PrevStatus[ServerStatusInfoMetadata.Listeners];
                }
            }

            return m_StoppedStatus;
        }

        public virtual StatusInfoCollection CollectServerStatus(StatusInfoCollection nodeStatus)
        {
            var appServer = AppServer;

            if (appServer == null)
            {
                var stoppedStatus = GetStoppedStatus();
                stoppedStatus.CollectedTime = DateTime.Now;
                return stoppedStatus;
            }

            var currentStatus = appServer.CollectServerStatus(nodeStatus);
            m_PrevStatus = currentStatus;
            return currentStatus;
        }

        public void Dispose()
        {
            Dispose(true);
            // Use SupressFinalize in case a subclass 
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Stop();
        }

        ~IsolationAppServer()
        {
            Dispose(false);
        }
    }
}
