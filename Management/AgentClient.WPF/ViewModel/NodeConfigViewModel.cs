using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Management.AgentClient.Config;
using System.Windows.Input;
using SuperSocket.Management.AgentClient.Command;
using System.Windows.Controls;

namespace SuperSocket.Management.AgentClient.ViewModel
{
    public class NodeConfigViewModel : ViewModelBase
    {
        public NodeConfigViewModel(NodeConfig node)
        {
            IsNew = node is NewNodeConfig;
            Name = IsNew ? string.Empty : node.Name;
            Uri = node.Uri;
            UserName = node.UserName;
            Password = node.Password;
            SaveCommand = new DelegateCommand(ExecuteSaveCommand, CanExecuteSaveCommand);
            RemoveCommand = new DelegateCommand(ExecuteRemoveCommand, CanExecuteRemoveCommand);
        }

        public bool IsNew { get; private set; }

        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                RaisePropertyChanged("Name");
                OnSaveCommandCanExecuteChanged();
            }
        }

        private string m_Uri;

        public string Uri
        {
            get { return m_Uri; }
            set
            {
                m_Uri = value;
                RaisePropertyChanged("Uri");
                OnSaveCommandCanExecuteChanged();
            }
        }

        private string m_UserName;

        public string UserName
        {
            get { return m_UserName; }
            set
            {
                m_UserName = value;
                RaisePropertyChanged("UserName");
                OnSaveCommandCanExecuteChanged();
            }
        }

        private string m_Password;

        public string Password
        {
            get { return m_Password; }
            set
            {
                m_Password = value;
                RaisePropertyChanged("Password");
                OnSaveCommandCanExecuteChanged();
            }
        }

        public DelegateCommand SaveCommand { get; private set; }

        private void OnSaveCommandCanExecuteChanged()
        {
            var command = SaveCommand;

            if (command != null)
                command.RaiseCanExecuteChanged();
        }

        private bool CanExecuteSaveCommand()
        {
            if (string.IsNullOrEmpty(Name))
                return false;

            if (string.IsNullOrEmpty(Uri))
                return false;

            if (string.IsNullOrEmpty(UserName))
                return false;

            if (string.IsNullOrEmpty(Password))
                return false;

            return true;
        }

        private void ExecuteSaveCommand()
        {
            var saved = Saved;

            if (saved != null)
                saved(this, EventArgs.Empty);
        }

        public event EventHandler Saved;

        public DelegateCommand RemoveCommand { get; private set; }

        private bool CanExecuteRemoveCommand()
        {
            return !IsNew;
        }

        private void ExecuteRemoveCommand()
        {
            if (!CanExecuteRemoveCommand())
                return;

            var removed = Removed;

            if (removed != null)
                removed(this, EventArgs.Empty);
        }

        public event EventHandler Removed;
    }
}
