using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Xml.Serialization;
using SuperSocket.Management.Client.Config;
using WebSocket4Net;

namespace SuperSocket.Management.Client.ViewModel
{
    public class ServerViewModel : ViewModelBase
    {
        public ServerViewModel()
        {
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
