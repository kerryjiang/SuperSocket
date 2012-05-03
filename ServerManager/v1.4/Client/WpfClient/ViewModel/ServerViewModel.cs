using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.ClientEngine;
using SuperSocket.Management.Client.Config;
using SuperSocket.Management.Shared;
using WebSocket4Net;
using GalaSoft.MvvmLight.Command;

namespace SuperSocket.Management.Client.ViewModel
{
    public class ServerViewModel : ViewModelBase
    {
        [XmlIgnore]
        public List<InstanceViewModel> Instances { get; private set; }

        private ServerConfig m_ServerConfig;

        private bool m_ErrorPopped = false;

        internal ServerConfig Config
        {
            get { return m_ServerConfig; }
        }

        private JsonWebSocket m_WebSocket;

        private Timer m_ReconnectTimer;

        public ServerViewModel(ServerConfig config)
        {
            Instances = new List<InstanceViewModel>();
            m_ServerConfig = config;
            Name = config.Name;
            m_WebSocket = CreateWebSocket(config);
            m_ReconnectTimer = new Timer(ReconnectTimerCallback, null, Timeout.Infinite, Timeout.Infinite);

            ConfigCommand = new RelayCommand<object>(ExecuteConfigCommand);
        }

        private JsonWebSocket CreateWebSocket(ServerConfig config)
        {
            var websocket = new JsonWebSocket(string.Format("wss://{0}:{1}/", config.Host, config.Port), "ServerManager");
#if DEBUG
            websocket.AllowUnstrustedCertificate = true;
#endif
            websocket.Opened += new EventHandler(m_WebSocket_Opened);
            websocket.Error += new EventHandler<ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
            websocket.Closed += new EventHandler(m_WebSocket_Closed);
            websocket.On<ServerInfo>(CommandName.UPDATE, OnServerUpdated);
            websocket.Open();
            State = ConnectionState.Connecting;
            m_ErrorPopped = false;

            foreach (var instance in Instances)
            {
                instance.State = InstanceState.Connecting;
            }

            return websocket;
        }

        void m_WebSocket_Closed(object sender, EventArgs e)
        {
            SetClosedStatus();
            WaitingReconnect();
        }

        void WaitingReconnect()
        {
            m_ReconnectTimer.Change(2 * 1000 * 60, Timeout.Infinite);
            State = ConnectionState.WaitingReconnect;
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
            if (State == ConnectionState.Connecting)
            {
                m_WebSocket = null;
                WaitingReconnect();
            }

            if (!m_ErrorPopped)
            {
                m_ErrorPopped = true;
                Messenger.Default.Send<ErrorEventArgs>(e);
            }
        }

        internal void RefreshConfig()
        {
            this.Name = m_ServerConfig.Name;

            if (m_WebSocket != null)
            {
                m_WebSocket.Closed -= m_WebSocket_Closed;
                m_WebSocket.Closed += new EventHandler(m_WebSocketRefresh_Closed);
                m_WebSocket.Close();
            }
            else
            {
                m_WebSocket = CreateWebSocket(m_ServerConfig);
            }
        }

        internal void StopConnection()
        {
            if (m_WebSocket != null)
            {
                m_WebSocket.Closed -= m_WebSocket_Closed;
                m_WebSocket.Close();
            }
        }

        void SetClosedStatus()
        {
            SetClosedStatus(ConnectionState.NotConnected);
        }

        void SetClosedStatus(ConnectionState closeState)
        {
            State = closeState;
            m_WebSocket = null;

            foreach (var instance in Instances)
            {
                instance.IsRunning = false;
                instance.State = InstanceState.NotConnected;
            }
        }

        void m_WebSocketRefresh_Closed(object sender, EventArgs e)
        {
            SetClosedStatus();
            m_WebSocket = CreateWebSocket(m_ServerConfig);
        }

        void m_WebSocket_Opened(object sender, EventArgs e)
        {
            var websocket = sender as JsonWebSocket;
            websocket.Query<LoginResult>(CommandName.LOGIN, new LoginInfo { UserName = m_ServerConfig.UserName, Password = m_ServerConfig.Password }, OnServerLoggedIn);
        }

        private void OnServerLoggedIn(LoginResult result)
        {
            //Login failed
            if (!result.Result)
            {
                string faultMessage = this.Name + ": user name or password doesn't match";

                if (m_WebSocket != null)
                {
                    m_WebSocket.Closed -= m_WebSocket_Closed;
                    m_WebSocket.Closed += (s, e) =>
                    {
                        SetClosedStatus(ConnectionState.Fault);
                        Description = faultMessage;
                        Instances.Clear();
                        Messenger.Default.Send<IEnumerable<InstanceViewModel>>(null);
                    };
                    m_WebSocket.Close();
                }
                
                Messenger.Default.Send<AlertMessage>(new AlertMessage(faultMessage));
                return;
            }

            State = ConnectionState.Connected;
            OnServerUpdated(result.ServerInfo);
        }

        private string m_Description;

        public string Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
                RaisePropertyChanged("Description");
            }
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
                targetInstance.Listener = instance.Listener;
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

        private ConnectionState m_State = ConnectionState.None;

        [XmlIgnore]
        public ConnectionState State
        {
            get { return m_State; }
            set
            {
                m_State = value;
                RaisePropertyChanged("State");
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

        public RelayCommand<object> ConfigCommand { get; private set; }

        private void ExecuteConfigCommand(object target)
        {
            Messenger.Default.Send<ConfigCommandMessage>(new ConfigCommandMessage(this));
        }
    }
}
