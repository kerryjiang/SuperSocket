using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;

namespace SuperSocket.Management.Client
{
    /// <summary>
    /// Interaction logic for NewServer.xaml
    /// </summary>
    public partial class NewEditServer : UserControl
    {
        public NewEditServer()
        {
            InitializeComponent();
            Messenger.Default.Register<NewEditServerMessage>(this, OnNewEditServerMessage);
        }

        private void OnNewEditServerMessage(NewEditServerMessage message)
        {
            Dispatcher.Invoke((Action<Window, NewEditServerMessage>)((w, m) => MessageBox.Show(w, m.Message)), (Window)this.Parent, message);
        }
    }
}
