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
using System.Windows.Forms;

namespace SuperSocket.Management.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon m_NotifyIcon;

        private bool m_QuitFromSystemTray = false;

        public MainWindow()
        {
            InitializeComponent();

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
            m_QuitFromSystemTray = true;
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
            if (!m_QuitFromSystemTray)
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
    }
}
