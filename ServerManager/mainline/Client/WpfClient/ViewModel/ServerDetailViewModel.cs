using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.Management.Client.Config;
using System.Security.Cryptography;

namespace SuperSocket.Management.Client.ViewModel
{
    public abstract class ServerDetailViewModel : MyViewModelBase
    {
        protected ServerDetailViewModel(bool isEdit)
        {
            IsEdit = isEdit;
            SaveCommand = new RelayCommand<object>(ExecuteSaveCommand, CanExecuteSaveCommand);
        }

        public bool IsEdit { get; private set; }

        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string m_Host;

        public string Host
        {
            get { return m_Host; }
            set
            {
                m_Host = value;
                RaisePropertyChanged("Host");
            }
        }

        private int? m_Port;

        public int? Port
        {
            get { return m_Port; }
            set
            {
                m_Port = value;
                RaisePropertyChanged("Port");
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
            }
        }

        public RelayCommand<object> CloseCommand { get; protected set; }

        public RelayCommand<object> SaveCommand { get; private set; }

        public bool PasswordBoxDefaultEnabled
        {
            get { return !IsEdit; }
        }

        protected abstract void ExecuteSaveCommand(object target);

        private bool CanExecuteSaveCommand(object target)
        {
            if (string.IsNullOrEmpty(Name))
                return false;

            if (string.IsNullOrEmpty(Host))
                return false;

            if (!Port.HasValue || Port.Value <= 0)
                return false;

            if (string.IsNullOrEmpty(UserName))
                return false;

            var passwordBox = target as PasswordBox;

            if (passwordBox != null && passwordBox.IsEnabled && string.IsNullOrEmpty(passwordBox.Password))
                return false;

            return true;
        }

        protected string EncryptPassword(string password)
        {
#if SILVERLIGHT
            return Silverlight.GetSHA1Hash(password);
#else
            return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(password)));
#endif
        }

#if SILVERLIGHT
        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            SaveCommand.RaiseCanExecuteChanged();
        }
#endif
    }
}
