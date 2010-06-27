using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ServiceConsole.ClientBase;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace SuperSocket.ServiceConsole.ClientShell.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private ObservableCollection<ServerLocationViewModel> m_ServerLocations;

        public ObservableCollection<ServerLocationViewModel> ServerLocations
        {
            get { return m_ServerLocations; }
            set
            {
                m_ServerLocations = value;
                OnPropertyChanged("ServerLocations");
            }
        }

        public MainViewModel()
        {
            var serverLocations = new List<ServerLocationViewModel>();

            foreach (var s in GetServerLocationFromConfig())
            {
                serverLocations.Add(new ServerLocationViewModel(s));
            }

            ServerLocations = new ObservableCollection<ServerLocationViewModel>(serverLocations);
        }

        private List<ServerLocation> GetServerLocationFromConfig()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<ServerLocation>));
            List<ServerLocation> servers;

            if (!File.Exists("server.xml"))
            {
                servers = new List<ServerLocation>
                {
                    new ServerLocation
                    {
                        Name = "local",
                        Address = "localhost"
                    }
                };

                using (StreamWriter writer = new StreamWriter("server.xml", false, Encoding.UTF8))
                {
                    serializer.Serialize(writer, servers);
                    writer.Flush();
                    writer.Close();
                }

                return servers;
            }

            using (StreamReader reader = new StreamReader("server.xml", Encoding.UTF8, true))
            {
                servers = (List<ServerLocation>)serializer.Deserialize(reader);
                reader.Close();
            }

            return servers;
        }
    }
}
