using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.ViewModel
{
    public class InstanceViewModel : InstanceViewModelBase
    {
        private bool m_InOperation = false;
        private string m_OperationToken;

        public InstanceViewModel(ServerViewModel server, string name)
            : base(server)
        {
            StartCommand = new RelayCommand<object>(ExecuteStart, CanStart);
            StopCommand = new RelayCommand<object>(ExecuteStop, CanStop);
            Name = name;
        }

        private void ExecuteStart(object state)
        {
            State = InstanceState.Starting;
            m_OperationToken = Server.StartInstance(this.Name, OperationCallback);
        }

        private bool CanStart(object state)
        {
            if (m_InOperation)
                return false;

            if (State == InstanceState.NotConnected)
                return false;

            return !IsRunning;
        }

        private void ExecuteStop(object state)
        {
            State = InstanceState.Stopping;
            m_OperationToken = Server.StopInstance(this.Name, OperationCallback);
        }

        private void OperationCallback(string token)
        {
            if (!m_OperationToken.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                //TODO: do what?
                return;
            }

            m_InOperation = false;
            m_OperationToken = string.Empty;

            if (IsRunning)
                State = InstanceState.Running;
            else
                State = InstanceState.NotStarted;
        }

        private bool CanStop(object state)
        {
            if (m_InOperation)
                return false;

            if (State == InstanceState.NotConnected)
                return false;

            return IsRunning;
        }

        public string Name { get; private set; }

        private bool m_IsRunning;

        public bool IsRunning
        {
            get { return m_IsRunning; }
            set
            {
                m_IsRunning = value;
                RaisePropertyChanged("IsRunning");
            }
        }

        private DateTime m_StartedTime;

        public DateTime StartedTime
        {
            get { return m_StartedTime; }
            set
            {
                m_StartedTime = value;
                RaisePropertyChanged("StartedTime");
            }
        }

        private int m_MaxConnectionCount;

        public int MaxConnectionCount
        {
            get { return m_MaxConnectionCount; }
            set
            {
                m_MaxConnectionCount = value;
                RaisePropertyChanged("MaxConnectionCount");
            }
        }

        private int m_CurrentConnectionCount;

        public int CurrentConnectionCount
        {
            get { return m_CurrentConnectionCount; }
            set
            {
                m_CurrentConnectionCount = value;
                RaisePropertyChanged("CurrentConnectionCount");
            }
        }

        public string Listener { get; set; }

        private int m_RequestHandlingSpeed;

        public int RequestHandlingSpeed
        {
            get { return m_RequestHandlingSpeed; }
            set
            {
                m_RequestHandlingSpeed = value;
                RaisePropertyChanged("RequestHandlingSpeed");
            }
        }

        private InstanceState m_State;

        public InstanceState State
        {
            get { return m_State; }
            set
            {
                m_State = value;
                RaisePropertyChanged("State");
            }
        }

        public RelayCommand<object> StartCommand { get; private set; }

        public RelayCommand<object> StopCommand { get; private set; }
    }
}
