using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.ClientEngine;
using SuperSocket.Management.Client.ViewModel;

namespace SuperSocket.Management.Client
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            Messenger.Default.Register<ErrorEventArgs>(this, OnError);
            Messenger.Default.Register<NewServerMessage>(this, OnNewServerMessage);
            Messenger.Default.Register<ConfigCommandMessage>(this, OnConfigCommandMessage);
            Messenger.Default.Register<AlertMessage>(this, HandleAlertMessage);
        }

        private void OnError(ErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<ErrorEventArgs>((i) =>
            {
                System.Windows.MessageBox.Show(i.Exception.Message);
            }), e);
        }

        private void OnNewServerMessage(NewServerMessage message)
        {
            var window = new ChildWindow();
            var contentControl = new NewEditServer();
            contentControl.DataContext = new NewServerDetailViewModel();
            window.Content = contentControl;
            window.Closed += (s, e) =>
                {
                    Messenger.Default.Unregister<CloseNewServerMessage>(this);
                    Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
                };

            Messenger.Default.Register<CloseNewServerMessage>(this, (m) =>
            {
                window.DialogResult = false;
                window.Close();
            });

            window.Show();
        }

        private void OnConfigCommandMessage(ConfigCommandMessage message)
        {
            var window = new ChildWindow();
            var contentControl = new NewEditServer();
            contentControl.DataContext = new EditServerDetailViewModel(message.Server);
            window.Content = contentControl;
            window.Closed += (s, e) =>
            {
                Messenger.Default.Unregister<CloseEditServerMessage>(this);
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };

            Messenger.Default.Register<CloseEditServerMessage>(this, (m) =>
            {
                window.DialogResult = false;
                window.Close();
            });

            window.Show();
        }

        private void HandleAlertMessage(AlertMessage message)
        {
            Dispatcher.BeginInvoke((Action<AlertMessage>)((m) =>
            {
                System.Windows.MessageBox.Show(m.Message);
            }), message);
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as DataGrid;

            IEnumerable<UIElement> elementsUnderMouse = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), this);

            DataGridRow row = elementsUnderMouse
                    .Where(uie => uie is DataGridRow)
                    .Cast<DataGridRow>()
                    .FirstOrDefault();

            if (row != null)
            {
                grid.SelectedItem = row.DataContext;
                e.Handled = false;
            }
        }
    }
}
