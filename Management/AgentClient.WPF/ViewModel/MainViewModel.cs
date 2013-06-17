using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.AgentClient.Config;
using System.Collections.ObjectModel;

namespace SuperSocket.Management.AgentClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        internal AgentConfig AgentConfig { get; private set; }

        public MainViewModel(AgentConfig config)
        {
            AgentConfig = config;
            var nodes = config.Nodes;

            if (nodes != null)
                Nodes = new ObservableCollection<NodeMasterViewModel>(nodes.Select(n => new NodeMasterViewModel(n)));
            else
                Nodes = new ObservableCollection<NodeMasterViewModel>();
        }

        private ObservableCollection<NodeMasterViewModel> m_Nodes;

        public ObservableCollection<NodeMasterViewModel> Nodes
        {
            get { return m_Nodes; }
            set
            {
                m_Nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }

        internal void OnNodeUpdated(object sender, EventArgs e)
        {
            var node = sender as NodeConfig;

            for (var i = 0; i < Nodes.Count; i++)
            {
                var targetNode = Nodes[i];

                if (targetNode.Config == node)
                {
                    Nodes[i] = new NodeMasterViewModel(node);
                    break;
                }
            }
        }

        internal void OnNodeRemoved(object sender, EventArgs e)
        {
            var node = sender as NodeConfig;

            for (var i = Nodes.Count - 1; i >= 0; i--)
            {
                var targetNode = Nodes[i];

                if (targetNode.Config == node)
                {
                    try
                    {
                        Nodes.RemoveAt(i);
                    }
                    catch
                    {
                    }
                    break;
                }
            }
        }

        internal void OnNodeAdded(object sender, EventArgs e)
        {
            var node = sender as NodeConfig;
            Nodes.Add(new NodeMasterViewModel(node));
        }
    }
}
