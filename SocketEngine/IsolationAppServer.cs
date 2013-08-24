using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Metadata;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketEngine
{
    abstract class IsolationAppServer : MarshalByRefObject, IWorkItem, IStatusInfoSource, IExceptionSource, IDisposable
    {
        protected const string WorkingDir = "AppRoot";

        protected string ServerTypeName { get; private set; }

        protected IBootstrap Bootstrap { get; private set; }

        protected IServerConfig ServerConfig { get; private set; }

        protected ProviderFactoryInfo[] Factories { get; private set; }

        protected IWorkItemBase AppServer { get; private set; }

        public string Name { get; private set; }

        private StatusInfoAttribute[] m_ServerStatusMetadata;

        private AutoResetEvent m_StopResetEvent = new AutoResetEvent(false);

        protected IsolationAppServer(string serverTypeName, StatusInfoAttribute[] serverStatusMetadata)
        {
            State = ServerState.NotInitialized;
            ServerTypeName = serverTypeName;
            m_ServerStatusMetadata = PrepareStatusMetadata(serverStatusMetadata);
        }

        /// <summary>
        /// Gets a value indicating whether [status metadata extended].
        /// </summary>
        /// <value>
        /// <c>true</c> if [status metadata extended]; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool StatusMetadataExtended
        {
            get { return false; }
        }

        private StatusInfoAttribute[] PrepareStatusMetadata(StatusInfoAttribute[] serverStatusMetadata)
        {
            if (!StatusMetadataExtended)
                return serverStatusMetadata;

            var additionalAttrs = this.GetType()
                            .GetCustomAttributes(typeof(StatusInfoAttribute), true)
                            .OfType<StatusInfoAttribute>()
                            .ToArray();

            if (additionalAttrs.Length == 0)
                return serverStatusMetadata;

            var list = serverStatusMetadata.ToList();
            list.AddRange(additionalAttrs);
            return list.ToArray();
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
            State = ServerState.NotStarted;

            return true;
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
            m_StopResetEvent.WaitOne();
        }

        protected virtual void OnStopped()
        {
            State = ServerState.NotStarted;
            AppServer = null;
            m_StopResetEvent.Set();
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
            if (m_StoppedStatus == null)
            {
                m_StoppedStatus = new StatusInfoCollection();
                m_StoppedStatus.Name = Name;
                m_StoppedStatus.Tag = Name;
                m_StoppedStatus[StatusInfoKeys.IsRunning] = false;
                m_StoppedStatus[StatusInfoKeys.MaxConnectionNumber] = ServerConfig.MaxConnectionNumber;

                if (m_PrevStatus != null)
                {
                    m_StoppedStatus[StatusInfoKeys.Listeners] = m_PrevStatus[StatusInfoKeys.Listeners];
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

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// Return null, never expired
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.
        /// </returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
        ///   </PermissionSet>
        public override object InitializeLifetimeService()
        {
            return null;
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

        public event EventHandler<SuperSocket.Common.ErrorEventArgs> ExceptionThrown;

        protected void OnExceptionThrown(Exception exc)
        {
            var handler = ExceptionThrown;

            if (handler == null)
                return;

            handler(this, new SuperSocket.Common.ErrorEventArgs(exc));
        }

        public void TransferSystemMessage(string messageType, object messageData)
        {
            var appServer = AppServer;

            if (appServer == null)
                OnExceptionThrown(new Exception("You cannot send system message to the instance who is not started."));

            appServer.TransferSystemMessage(messageType, messageData);
        }
    }
}
