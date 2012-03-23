using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace SuperSocket.Management.Client.ViewModel
{
    public class NewServerViewModel : ViewModelBase
    {
        public NewServerViewModel()
        {
            CloseCommand = new RelayCommand<object>((s) => Messenger.Default.Send<CloseNewServerMessage>(CloseNewServerMessage.Empty));
        }

        public RelayCommand<object> CloseCommand { get; private set; }

        public RelayCommand<object> SaveCommand { get; private set; }
    }
}
