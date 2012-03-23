using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace SuperSocket.Management.Client.ViewModel
{
    public class NewServerViewModel : ViewModelBase
    {
        public RelayCommand<object> CloseCommand { get; private set; }

        public RelayCommand<object> SaveCommand { get; private set; }
    }
}
