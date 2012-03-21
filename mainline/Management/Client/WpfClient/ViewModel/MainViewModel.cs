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
using System.Windows.Threading;
using System.Threading;

namespace SuperSocket.Management.Client.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private List<ServerViewModel> m_Servers;

        public MainViewModel()
        {
            Messenger.Default.Register<IEnumerable<InstanceViewModel>>(this, OnNewInstancesFound);
            StartGetServers(App.ClientConfig);
        }

        private void StartGetServers(ClientAppConfig appConfig)
        {
            m_Servers = new List<ServerViewModel>();

            foreach (var serverConfig in appConfig.Servers)
            {
                var server = new ServerViewModel(serverConfig);
                m_Servers.Add(server);
            }
        }

        private void OnNewInstancesFound(IEnumerable<InstanceViewModel> instances)
        {
            Instances = null;
        }

        public List<InstanceViewModelBase> Instances
        {
            get
            {
                var instances = new List<InstanceViewModelBase>();

                foreach (var server in m_Servers)
                {
                    if (!server.Instances.Any())
                    {
                        instances.Add(new LoadingInstanceViewModel(server));
                        continue;
                    }

                    instances.AddRange(server.Instances);
                }

                return instances;
            }
            set
            {
                RaisePropertyChanged("Instances");
            }
        }

        public override void Cleanup()
        {
            Messenger.Default.Unregister<IEnumerable<InstanceViewModel>>(this, OnNewInstancesFound);
            base.Cleanup();
        }
    }
}
