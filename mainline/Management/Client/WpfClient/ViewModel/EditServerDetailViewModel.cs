using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.Management.Client.Config;

namespace SuperSocket.Management.Client.ViewModel
{
    public class EditServerDetailViewModel : ServerDetailViewModel
    {
        private ServerViewModel m_Server;

        public EditServerDetailViewModel(ServerViewModel server)
            : base(true)
        {
            m_Server = server;

            this.Name = server.Name;
            this.Host = server.Config.Host;
            this.Port = server.Config.Port;
            this.UserName = server.Config.UserName;

            CloseCommand = new RelayCommand<object>((s) => Messenger.Default.Send<CloseEditServerMessage>(CloseEditServerMessage.Empty));
        }

        protected override void ExecuteSaveCommand(object target)
        {
            ServerConfig server = m_Server.Config;

            server.Name = Name;
            server.Host = Host;
            server.Port = Port.Value;
            server.UserName = UserName;

            var passwordBox = target as PasswordBox;
            server.Password = passwordBox.Password;

            App.SaveConfig();

            Messenger.Default.Send<CloseEditServerMessage>(CloseEditServerMessage.Empty);

            m_Server.RefreshConfig();
        }
    }
}
