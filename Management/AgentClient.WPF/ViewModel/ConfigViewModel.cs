using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.AgentClient.Config;
using System.Collections.ObjectModel;

namespace SuperSocket.Management.AgentClient.ViewModel
{
    public class NewNodeConfig : NodeConfig
    {
        public NewNodeConfig()
        {
            Name = "* New Node";
        }
    }

    public class ConfigViewModel : ViewModelBase
    {
        private AgentConfig m_AgentConfig;

        private ObservableCollection<NodeConfig> m_Nodes;

        public ObservableCollection<NodeConfig> Nodes
        {
            get { return m_Nodes; }
            set
            {
                m_Nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }

        public ConfigViewModel(AgentConfig agentConfig)
        {
            m_AgentConfig = agentConfig;
            m_Nodes = new ObservableCollection<NodeConfig>(agentConfig.Nodes);
            m_Nodes.Add(new NewNodeConfig());
            SelectedNode = m_Nodes.First();
        }

        private NodeConfig m_SelectedNode;

        public NodeConfig SelectedNode
        {
            get { return m_SelectedNode; }
            set
            {
                m_SelectedNode = value;
                RaisePropertyChanged("SelectedNode");
                SelectedNodeViewModel = new NodeConfigViewModel(value);
            }
        }

        private NodeConfigViewModel m_SelectedNodeViewModel;

        public NodeConfigViewModel SelectedNodeViewModel
        {
            get { return m_SelectedNodeViewModel; }
            set
            {
                m_SelectedNodeViewModel = value;
                if (m_SelectedNodeViewModel != null)
                {
                    m_SelectedNodeViewModel.Saved += new EventHandler(OnNodeViewModelSaved);
                    m_SelectedNodeViewModel.Removed += new EventHandler(OnNodeViewModelRemoved);
                }
                RaisePropertyChanged("SelectedNodeViewModel");
            }
        }

        void OnNodeViewModelRemoved(object sender, EventArgs e)
        {
            var currentNode = SelectedNode;
            
            //Remove
            m_Nodes.Remove(currentNode);
            m_AgentConfig.Nodes.Remove(currentNode);

            //Select a new node
            SelectedNode = m_Nodes.First();

            //Save
            m_AgentConfig.Save();

            FireEventHandler(Removed, currentNode);
        }

        void OnNodeViewModelSaved(object sender, EventArgs e)
        {
            var nodeViewMode = sender as NodeConfigViewModel;
            var node = new
            {
                Name = nodeViewMode.Name,
                Uri = nodeViewMode.Uri,
                UserName = nodeViewMode.UserName,
                Password = nodeViewMode.Password
            };

            var currentNode = SelectedNode;

            var isNew = false;

            if (currentNode is NewNodeConfig)
            {
                var newNode = new NodeConfig();
                m_Nodes.Add(newNode);
                SelectedNode = newNode;

                //Update UI
                m_Nodes.Remove(currentNode);
                currentNode = newNode;

                m_Nodes.Add(new NewNodeConfig());

                //Update configuration model
                m_AgentConfig.Nodes.Add(currentNode);

                isNew = true;
            }

            currentNode.Name = node.Name;
            currentNode.Uri = node.Uri;
            currentNode.UserName = node.UserName;
            currentNode.Password = node.Password;

            //Save
            m_AgentConfig.Save();

            //Added
            if (isNew)
            {
                FireEventHandler(Added, currentNode);
            }
            else
            {
                FireEventHandler(Updated, currentNode);
            }
        }

        private void FireEventHandler(EventHandler handler, object sender)
        {
            if (handler != null)
                handler(sender, EventArgs.Empty);
        }

        public event EventHandler Updated;

        public event EventHandler Added;

        public event EventHandler Removed;
    }
}
