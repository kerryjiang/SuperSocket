using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ServiceConsole.ClientBase;
using System.Collections.ObjectModel;

namespace SuperSocket.ServiceConsole.ClientShell.ViewModel
{
    class ServerLocationViewModel : ViewModelBase
    {
        private ServerLocation m_ServerLocation;

        public ServerLocationViewModel(ServerLocation location)
        {
            m_ServerLocation = location;
            m_ServerLocation.Status = ServerLocationStatus.Disconnected;
        }

        public string Name
        {
            get { return m_ServerLocation.Name; }
        }

        public string Address
        {
            get { return m_ServerLocation.Address; }
        }

        public ServerLocationStatus Status
        {
            get
            {
                return m_ServerLocation.Status;
            }
            set
            {
                m_ServerLocation.Status = value;
                OnPropertyChanged("Status");
            }
        }

        private ObservableCollection<Server> m_Servers;

        public ObservableCollection<Server> Servers
        {
            get { return m_Servers; }
            set
            {
                m_Servers = value;
                OnPropertyChanged("Servers");
            }
        }
    }
}
