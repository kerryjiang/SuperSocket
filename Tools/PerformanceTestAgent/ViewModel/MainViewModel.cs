using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace PerformanceTestAgent.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            m_StartCommand = new RelayCommand(ExecuteStart, CanExecuteStart);
        }

        private string m_TargetServer;

        public string TargetServer
        {
            get { return m_TargetServer; }
            set
            {
                m_TargetServer = value;
                RaisePropertyChanged("TargetServer");
            }
        }

        private int m_Port;

        public int Port
        {
            get { return m_Port; }
            set
            {
                m_Port = value;
                RaisePropertyChanged("Port");
            }
        }

        private int m_ThreadCount;

        public int ThreadCount
        {
            get { return m_ThreadCount; }
            set
            {
                m_ThreadCount = value;
                RaisePropertyChanged("ThreadCount");
            }
        }

        private bool m_IsRunning = false;

        public bool IsRunning
        {
            get { return m_IsRunning; }
            set
            {
                m_IsRunning = value;
                RaisePropertyChanged("IsRunning");
            }
        }

        private bool m_IsStopped;

        public bool IsStopped
        {
            get { return m_IsStopped; }
            set
            {
                m_IsStopped = value;
                RaisePropertyChanged("IsStopped");
            }
        }

        private ICommand m_StartCommand;

        public ICommand StartCommand
        {
            get { return m_StartCommand; }
        }

        private void ExecuteStart()
        {

        }

        private bool CanExecuteStart()
        {
            if (string.IsNullOrEmpty(m_TargetServer))
                return false;

            if (m_Port <= 0)
                return false;

            if (m_ThreadCount <= 0)
                return false;

            return true;
        }
    }
}
