using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using SuperSocket.ClientEngine;
using SuperSocket.Management.Client.Config;
using WebSocket4Net;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.ViewModel
{
    public class ServerViewModel : ViewModelBase
    {
        private List<InstanceViewModel> m_Instances = new List<InstanceViewModel>();

        private ServerConfig m_ServerConfig;

        private JsonWebSocket m_WebSocket;

        public ServerViewModel(ServerConfig config)
        {
            m_ServerConfig = config;
            Name = config.Name;
            m_WebSocket = CreateWebSocket(config);
        }

        private JsonWebSocket CreateWebSocket(ServerConfig config)
        {
            var websocket = new JsonWebSocket(config.Uri, "ServerManager");
            websocket.Opened += new EventHandler(m_WebSocket_Opened);
            websocket.Error += new EventHandler<ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
            websocket.Closed += new EventHandler(m_WebSocket_Closed);
            websocket.On<ServerInfo>(CommandName.UPDATE, OnServerUpdated);
            websocket.Open();
            return websocket;
        }

        void m_WebSocket_Closed(object sender, EventArgs e)
        {
            Connected = false;
            m_WebSocket = CreateWebSocket(m_ServerConfig);
        }

        void m_WebSocket_Error(object sender, ErrorEventArgs e)
        {

        }

        void m_WebSocket_Opened(object sender, EventArgs e)
        {
            var websocket = sender as JsonWebSocket;
            websocket.Query<LoginResult>(CommandName.LOGIN, new LoginInfo { UserName = m_ServerConfig.UserName, Password = m_ServerConfig.Password }, OnServerLoggedIn);
        }

        private void OnServerLoggedIn(LoginResult result)
        {
            if (result.Result)
                return;

            Connected = true;
            OnServerUpdated(result.ServerInfo);
        }

        private void OnServerUpdated(ServerInfo serverInfo)
        {
            this.AvailableCompletionPortThreads = serverInfo.AvailableCompletionPortThreads;
            this.AvailableWorkingThreads = serverInfo.AvailableWorkingThreads;
            this.CpuUsage = serverInfo.CpuUsage;
            this.MaxCompletionPortThreads = serverInfo.MaxCompletionPortThreads;
            this.MaxWorkingThreads = serverInfo.MaxWorkingThreads;
            this.PhysicalMemoryUsage = serverInfo.PhysicalMemoryUsage;
            this.TotalThreadCount = serverInfo.TotalThreadCount;
            this.VirtualMemoryUsage = serverInfo.VirtualMemoryUsage;

            foreach (var instance in serverInfo.Instances)
            {
                var targetInstance = m_Instances.FirstOrDefault(i => i.Name.Equals(instance.Name, StringComparison.OrdinalIgnoreCase));

                var newFound = false;

                if (targetInstance == null)
                {
                    targetInstance = new InstanceViewModel(this);
                    newFound = true;
                }

                targetInstance.CurrentConnectionCount = instance.CurrentConnectionCount;
                targetInstance.IsRunning = instance.IsRunning;
                targetInstance.Listeners = instance.Listeners;
                targetInstance.MaxConnectionCount = instance.MaxConnectionCount;
                targetInstance.RequestHandlingSpeed = instance.RequestHandlingSpeed;
                targetInstance.StartedTime = instance.StartedTime;

                if (newFound)
                {
                    m_Instances.Add(targetInstance);
                    this.MessengerInstance.Send<InstanceViewModel>(targetInstance);
                }
            }
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
    }
}
