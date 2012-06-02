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
            Messenger.Default.Register<DialogMessage>(this, HandleDialogMessage);
        }

        private void OnNewEditServerMessage(NewEditServerMessage message)
        {
            Dispatcher.Invoke((Action<Window, NewEditServerMessage>)((w, m) => MessageBox.Show(w, m.Message)), (Window)this.Parent, message);
        }

        private void HandleDialogMessage(DialogMessage message)
        {
            if (message == null)
            {
                return;
            }

            if (message.Sender == this.DataContext)
            {
                var result = MessageBox.Show(
                        (Window)(((Control)this.Parent).Parent),
                        message.Content,
                        message.Caption,
                        message.Button,
                        message.Icon,
                        message.DefaultResult,
                        message.Options);

                if (message.Callback != null)
                {
                    message.Callback(result);
                }
            }
        }
    }
}
