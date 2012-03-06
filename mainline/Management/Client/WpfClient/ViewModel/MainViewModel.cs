using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SuperSocket.Management.Shared;
using SuperSocket.Management.Client.Model;

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
            var serverModel = new ServerModel { Name = "localhost" };

            m_Instances = new ObservableCollection<InstanceModel>(new List<InstanceModel> { new LoadingInstanceModel
            {
                Server = serverModel
            } });
        }

        private ObservableCollection<InstanceModel> m_Instances;

        public ObservableCollection<InstanceModel> Instances
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
