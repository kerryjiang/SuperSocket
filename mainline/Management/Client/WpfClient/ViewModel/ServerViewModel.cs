using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.ClientEngine;
using SuperSocket.Management.Client.Config;
using SuperSocket.Management.Shared;
using WebSocket4Net;
using System.Threading;

namespace SuperSocket.Management.Client.ViewModel
{
    public class ServerViewModel : ViewModelBase
    {
        [XmlIgnore]
        public List<InstanceViewModel> Instances { get; private set; }

        private ServerConfig m_ServerConfig;

        private JsonWebSocket m_WebSocket;

        private Timer m_ReconnectTimer;

        public ServerViewModel(ServerConfig config)
        {
            Instances = new List<InstanceViewModel>();
            m_ServerConfig = config;
            Name = config.Name;
            m_WebSocket = CreateWebSocket(config);
            m_ReconnectTimer = new Timer(ReconnectTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        private JsonWebSocket CreateWebSocket(ServerConfig config)
        {
            var websocket = new JsonWebSocket(string.Format("ws://{0}:{1}/", config.Host, config.Port), "ServerManager");
            websocket.Opened += new EventHandler(m_WebSocket_Opened);
            websocket.Error += new EventHandler<ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
            websocket.Closed += new EventHandler(m_WebSocket_Closed);
            websocket.On<ServerInfo>(CommandName.UPDATE, OnServerUpdated);
            websocket.Open();

            foreach (var instance in Instances)
            {
                instance.State = InstanceState.Connecting;
            }

            return websocket;
        }

        void m_WebSocket_Closed(object sender, EventArgs e)
        {
            Connected = false;
            m_WebSocket = null;

            foreach (var instance in Instances)
            {
                instance.IsRunning = false;
                instance.State = InstanceState.NotConnected;
            }

            m_ReconnectTimer.Change(2 * 1000 * 60, Timeout.Infinite);
        }

        private void ReconnectTimerCallback(object state)
        {
            //Already connected or started connecting
            if (m_WebSocket != null)
                return;

            m_WebSocket = CreateWebSocket(m_ServerConfig);
        }

        void m_WebSocket_Error(object sender, ErrorEventArgs e)
        {
            Messenger.Default.Send<ErrorEventArgs>(e);
        }

        void m_WebSocket_Opened(object sender, EventArgs e)
        {
            var websocket = sender as JsonWebSocket;
            websocket.Query<LoginResult>(CommandName.LOGIN, new LoginInfo { UserName = m_ServerConfig.UserName, Password = m_ServerConfig.Password }, OnServerLoggedIn);
        }

        private void OnServerLoggedIn(LoginResult result)
        {
            if (!result.Result)
                return;

            Connected = true;
            OnServerUpdated(result.ServerInfo);
        }

        private void OnServerUpdated(ServerInfo serverInfo)
        {
            //The server doesn't have performance data yet
            if (serverInfo == null)
                return;

            this.AvailableCompletionPortThreads = serverInfo.AvailableCompletionPortThreads;
            this.AvailableWorkingThreads = serverInfo.AvailableWorkingThreads;
            this.CpuUsage = serverInfo.CpuUsage;
            this.MaxCompletionPortThreads = serverInfo.MaxCompletionPortThreads;
            this.MaxWorkingThreads = serverInfo.MaxWorkingThreads;
            this.PhysicalMemoryUsage = serverInfo.PhysicalMemoryUsage;
            this.TotalThreadCount = serverInfo.TotalThreadCount;
            this.VirtualMemoryUsage = serverInfo.VirtualMemoryUsage;

            var newInstanceFound = false;

            foreach (var instance in serverInfo.Instances)
            {
                var newFound = false;

                var targetInstance = Instances.FirstOrDefault(i => i.Name.Equals(instance.Name, StringComparison.OrdinalIgnoreCase));

                if (targetInstance == null)
                {
                    targetInstance = new InstanceViewModel(this, instance.Name);
                    newFound = true;
                    newInstanceFound = true;
                }

                targetInstance.CurrentConnectionCount = instance.CurrentConnectionCount;
                targetInstance.IsRunning = instance.IsRunning;
                targetInstance.Listeners = instance.Listeners;
                targetInstance.MaxConnectionCount = instance.MaxConnectionCount;
                targetInstance.RequestHandlingSpeed = instance.RequestHandlingSpeed;
                targetInstance.StartedTime = instance.StartedTime;

                if (newFound || (targetInstance.State == InstanceState.NotConnected || targetInstance.State == InstanceState.Connecting))
                {
                    targetInstance.State = targetInstance.IsRunning ? InstanceState.Running : InstanceState.NotStarted;
                }

                if (newFound)
                {
                    Instances.Add(targetInstance);
                }
            }

            if (newInstanceFound)
                Messenger.Default.Send<IEnumerable<InstanceViewModel>>(null);
        }

        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                RaisePropertyChanged("Name");
            }
        }

        private bool m_Connected;

        [XmlIgnore]
        public bool Connected
        {
            get { return m_Connected; }
            set
            {
                m_Connected = value;
                RaisePropertyChanged("Connected");
            }
        }

        private double m_CpuUsage;

        [XmlIgnore]
        public double CpuUsage
        {
            get { return m_CpuUsage; }
            set
            {
                m_CpuUsage = value;
                RaisePropertyChanged("CpuUsage");
            }
        }

        private int m_AvailableWorkingThreads;

        [XmlIgnore]
        public int AvailableWorkingThreads
        {
            get { return m_AvailableWorkingThreads; }
            set
            {
                m_AvailableWorkingThreads = value;
                RaisePropertyChanged("AvailableWorkingThreads");
            }
        }

        private int m_AvailableCompletionPortThreads;

        [XmlIgnore]
        public int AvailableCompletionPortThreads
        {
            get { return m_AvailableCompletionPortThreads; }
            set
            {
                m_AvailableCompletionPortThreads = value;
                RaisePropertyChanged("AvailableCompletionPortThreads");
            }
        }

        private int m_MaxWorkingThreads;

        [XmlIgnore]
        public int MaxWorkingThreads
        {
            get { return m_MaxWorkingThreads; }
            set
            {
                m_MaxWorkingThreads = value;
                RaisePropertyChanged("MaxWorkingThreads");
            }
        }

        private int m_MaxCompletionPortThreads;

        [XmlIgnore]
        public int MaxCompletionPortThreads
        {
            get { return m_MaxCompletionPortThreads; }
            set
            {
                m_MaxCompletionPortThreads = value;
                RaisePropertyChanged("MaxCompletionPortThreads");
            }
        }

        private double m_PhysicalMemoryUsage;

        [XmlIgnore]
        public double PhysicalMemoryUsage
        {
            get { return m_PhysicalMemoryUsage; }
            set
            {
                m_PhysicalMemoryUsage = value;
                RaisePropertyChanged("PhysicalMemoryUsage");
            }
        }

        private double m_VirtualMemoryUsage;

        [XmlIgnore]
        public double VirtualMemoryUsage
        {
            get { return m_VirtualMemoryUsage; }
            set
            {
                m_VirtualMemoryUsage = value;
                RaisePropertyChanged("VirtualMemoryUsage");
            }
        }

        private int m_TotalThreadCount;

        [XmlIgnore]
        public int TotalThreadCount
        {
            get { return m_TotalThreadCount; }
            set
            {
                m_TotalThreadCount = value;
                RaisePropertyChanged("TotalThreadCount");
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal string StartInstance(string instanceName, Action<string> tokenUpdater)
        {
            return m_WebSocket.Query<StartResult>(CommandName.START, instanceName, (t, r) =>
                {
                    OnServerUpdated(r.ServerInfo);
                    tokenUpdater(t);
                });
        }

        internal string StopInstance(string instanceName, Action<string> tokenUpdater)
        {
            return m_WebSocket.Query<StopResult>(CommandName.STOP, instanceName, (t, r) =>
                {
                    OnServerUpdated(r.ServerInfo);
                    tokenUpdater(t);
                });
        }

        public override void Cleanup()
        {
            if (m_ReconnectTimer != null)
            {
                m_ReconnectTimer.Dispose();
                m_ReconnectTimer = null;
            }

            base.Cleanup();
        }
    }
}
