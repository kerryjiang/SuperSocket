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
    public class NewServerDetailViewModel : ServerDetailViewModel
    {
        public NewServerDetailViewModel()
            : base(false)
        {
            CloseCommand = new RelayCommand<object>(ExecuteCloseCommand);
        }

        private void ExecuteCloseCommand(object target)
        {
            Messenger.Default.Send<CloseNewServerMessage>(CloseNewServerMessage.Empty);
        }

        protected override void ExecuteSaveCommand(object target)
        {
            ServerConfig server = null;

            var exsiting = App.ClientConfig.Servers.Any(s =>
                    s.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));

            if (exsiting)
            {
                Messenger.Default.Send<NewEditServerMessage>(new NewEditServerMessage { Message = "The server with same name already exists!" });
                return;
            }

            server = new ServerConfig();

            server.Name = Name;
            server.Host = Host;
            server.Port = Port.Value;
            server.UserName = UserName;

            var passwordBox = target as PasswordBox;
            server.Password = EncryptPassword(passwordBox.Password);

            Messenger.Default.Send<CloseNewServerMessage>(CloseNewServerMessage.Empty);
            Messenger.Default.Send<NewServerCreatedMessage>(new NewServerCreatedMessage(server));
        }
    }
}
