using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using SuperSocket.ClientEngine;
using SuperSocket.Management.Client.ViewModel;

namespace SuperSocket.Management.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon m_NotifyIcon;

        private bool m_ForceQuit = false;

        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<ErrorEventArgs>(this, OnError);
            Messenger.Default.Register<ExitMessage>(this, OnExitMessage);
            Messenger.Default.Register<NewServerMessage>(this, OnNewServerMessage);
            Messenger.Default.Register<ConfigCommandMessage>(this, OnConfigCommandMessage);
            Messenger.Default.Register<AlertMessage>(this, HandleAlertMessage);

            m_NotifyIcon = new System.Windows.Forms.NotifyIcon();
            m_NotifyIcon.Text = "SuperSocket Server Manager";
            m_NotifyIcon.Icon = Properties.Resources.Server;
            m_NotifyIcon.Visible = true;
            m_NotifyIcon.DoubleClick += new EventHandler(m_NotifyIcon_DoubleClick);

            var quitMenuItem = new System.Windows.Forms.MenuItem();
            quitMenuItem.Text = "&Exit";
            quitMenuItem.Click += new EventHandler(quitMenuItem_Click);

            var contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add(quitMenuItem);

            m_NotifyIcon.ContextMenu = contextMenu;
        }

        void quitMenuItem_Click(object sender, EventArgs e)
        {
            m_ForceQuit = true;
            this.Close();
        }

        void m_NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!m_ForceQuit)
            {
                e.Cancel = true;
                this.Hide();
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (m_NotifyIcon != null)
            {
                m_NotifyIcon.Visible = false;
                m_NotifyIcon = null;
            }

            base.OnClosed(e);
        }

        private void OnError(ErrorEventArgs e)
        {
            Dispatcher.Invoke(new Action<ErrorEventArgs>((i) =>
                {
                    System.Windows.MessageBox.Show(i.Exception.Message);
                }), e);
        }

        private void OnExitMessage(ExitMessage message)
        {
            if (System.Windows.MessageBox.Show(this, "Are you are you want to quit?", "Quit", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            m_ForceQuit = true;
            this.Close();
        }

        private void OnNewServerMessage(NewServerMessage message)
        {
            var window = new ChildWindow();
            var contentControl = new NewEditServer();
            contentControl.DataContext = new NewServerDetailViewModel();
            window.Content = contentControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.Topmost = true;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ResizeMode = ResizeMode.NoResize;
            window.Title = "New Server";

            Messenger.Default.Register<CloseNewServerMessage>(this, (m) =>
                {
                    window.DialogResult = false;
                    window.Close();
                });

            window.ShowDialog();
            Messenger.Default.Unregister<CloseNewServerMessage>(this);
        }

        private void OnConfigCommandMessage(ConfigCommandMessage message)
        {
            var window = new ChildWindow();
            var contentControl = new NewEditServer();
            contentControl.DataContext = new EditServerDetailViewModel(message.Server);
            window.Content = contentControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.Topmost = true;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ResizeMode = ResizeMode.NoResize;
            window.Title = "Edit Server";

            Messenger.Default.Register<CloseEditServerMessage>(this, (m) =>
            {
                window.DialogResult = false;
                window.Close();
            });

            window.ShowDialog();
            Messenger.Default.Unregister<CloseEditServerMessage>(this);
        }

        private void HandleAlertMessage(AlertMessage message)
        {
            Dispatcher.Invoke((Action<AlertMessage>)((m) =>
                {
                    System.Windows.MessageBox.Show(m.Message);
                }), message);
        }
    }
}
