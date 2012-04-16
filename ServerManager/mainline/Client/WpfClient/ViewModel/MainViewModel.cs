using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.Management.Client.Config;
using SuperSocket.Management.Shared;

namespace SuperSocket.Management.Client.ViewModel
{
    public class MainViewModel : MyViewModelBase
    {
        private List<ServerViewModel> m_Servers;

#if SILVERLIGHT
        private AsyncOperation m_AsyncOper = AsyncOperationManager.CreateOperation(null);
#endif

        public MainViewModel()
        {
            if (IsInDesignMode)
                return;

#if !SILVERLIGHT
            Messenger.Default.Register<IEnumerable<InstanceViewModel>>(this, OnNewInstancesFound);
            Messenger.Default.Register<NewServerCreatedMessage>(this, OnNewServerCreated);
            Messenger.Default.Register<ServerRemovedMessage>(this, OnServerRemovedMessage);
#else
            Messenger.Default.Register<IEnumerable<InstanceViewModel>>(this, CreateAsyncOperation<IEnumerable<InstanceViewModel>>(OnNewInstancesFound));
            Messenger.Default.Register<NewServerCreatedMessage>(this, CreateAsyncOperation<NewServerCreatedMessage>(OnNewServerCreated));
            Messenger.Default.Register<ServerRemovedMessage>(this, CreateAsyncOperation<ServerRemovedMessage>(OnServerRemovedMessage));
#endif

#if !SILVERLIGHT
            ExitCommand = new RelayCommand<object>(ExecuteExitCommand);
#endif
            ConnectNewServerCommand = new RelayCommand<object>(ExecuteConnectNewServerCommand);

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

        private void OnNewServerCreated(NewServerCreatedMessage message)
        {
            var servers = App.ClientConfig.Servers.ToList();
            servers.Add(message.Server);
            App.ClientConfig.Servers = servers.ToArray();

            var serverModel = new ServerViewModel(message.Server);
            m_Servers.Add(serverModel);
            Instances = null;

            App.SaveConfig();
        }

        private void OnServerRemovedMessage(ServerRemovedMessage message)
        {
            var servers = App.ClientConfig.Servers.ToList();
            servers.Remove(message.Server);
            App.ClientConfig.Servers = servers.ToArray();

            var server = m_Servers.FirstOrDefault(m => m.Config == message.Server);
            m_Servers.Remove(server);
            Instances = null;

            App.SaveConfig();
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
                    if (server.State == ConnectionState.Fault)
                    {
                        instances.Add(new FaultInstanceViewModel(server));
                    }
                    else if (server.Instances.Any())
                    {
#if SILVERLIGHT
                        instances.AddRange(server.Instances.OfType<InstanceViewModelBase>());
#else
                        instances.AddRange(server.Instances);
#endif
                    }
                    else
                    {
                        instances.Add(new LoadingInstanceViewModel(server));
                    }
                }

                return instances;
            }
            set
            {
                RaisePropertyChanged("Instances");
            }
        }

#if !SILVERLIGHT
        public RelayCommand<object> ExitCommand { get; private set; }

        private void ExecuteExitCommand(object target)
        {
            Messenger.Default.Send<ExitMessage>(ExitMessage.Empty);
        }
#endif

        public RelayCommand<object> ConnectNewServerCommand { get; private set; }

        private void ExecuteConnectNewServerCommand(object target)
        {
            Messenger.Default.Send<NewServerMessage>(NewServerMessage.Empty);
        }

        public override void Cleanup()
        {
            Messenger.Default.Unregister<IEnumerable<InstanceViewModel>>(this);
            Messenger.Default.Unregister<NewServerCreatedMessage>(this);
            Messenger.Default.Unregister<ServerRemovedMessage>(this);
            base.Cleanup();
        }
    }
}
