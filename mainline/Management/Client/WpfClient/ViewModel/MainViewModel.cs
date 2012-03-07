using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Task.Factory.StartNew(StartGetServers);
        }

        private void StartGetServers()
        {
            var serverModel = new ServerViewModel { Name = "localhost" };

            m_Instances = new ObservableCollection<InstanceRowViewModel>(new List<InstanceRowViewModel> { new LoadingInstanceRowViewModel
            {
                Server = serverModel
            } });
        }

        private ObservableCollection<InstanceRowViewModel> m_Instances;

        public ObservableCollection<InstanceRowViewModel> Instances
        {
            get { return m_Instances; }
            set
            {
                m_Instances = value;
                RaisePropertyChanged("Instances");
            }
        }
    }
}
