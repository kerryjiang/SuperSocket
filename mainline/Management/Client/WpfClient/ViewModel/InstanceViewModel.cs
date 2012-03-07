using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.ViewModel
{
    public class InstanceViewModel : ViewModelBase
    {
        public string Name { get; set; }

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

        public ListenerInfo[] Listeners { get; set; }


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
    }
}
