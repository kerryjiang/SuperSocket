using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SuperSocket.Management.Shared;
using SuperSocket.Management.Client.Config;
using GalaSoft.MvvmLight.Messaging;

namespace SuperSocket.Management.Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Task.Factory.StartNew((o) => StartGetServers((ClientAppConfig)o), App.ClientConfig);
            MessengerInstance = Messenger.Default;
            MessengerInstance.Register<InstanceViewModel>(this, OnNewInstanceFound);
        }

        private void StartGetServers(ClientAppConfig appConfig)
        {
            m_Instances = new ObservableCollection<InstanceViewModelBase>();

            foreach (var server in appConfig.Servers)
            {
                m_Instances.Add(new LoadingInstanceViewModel(new ServerViewModel(server)));
            }
        }

        private void OnNewInstanceFound(InstanceViewModel instance)
        {
            m_Instances.Where(i => i.Server.Name.Equals(instance.Name, StringComparison.OrdinalIgnoreCase) && i is LoadingInstanceViewModel);

            foreach (var loadingInstance in m_Instances)
            {
                m_Instances.Remove(loadingInstance);
            }

            m_Instances.Add(instance);
        }

        private ObservableCollection<InstanceViewModelBase> m_Instances;

        public ObservableCollection<InstanceViewModelBase> Instances
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
