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
using SuperSocket.ServerManager.Client.ViewModel;
using SuperSocket.ServerManager.Client.Config;

namespace SuperSocket.ServerManager.Client
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        public MainPanel()
        {
            InitializeComponent();
            this.Loaded += MainPanel_Loaded;
        }

        void MainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var mainViewModel = this.DataContext as MainViewModel;

            if (!mainViewModel.Nodes.Any())
            {
                ShowConfigDialog(mainViewModel);
            }
        }

        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            ShowConfigDialog(this.DataContext as MainViewModel);
        }

#if SILVERLIGHT
        private void ShowConfigDialog(MainViewModel mainViewModel)
        {
            var win = new ChildWindow();
            win.Title = "Configure";

            var configViewModel = new ConfigViewModel(mainViewModel.AgentConfig);
            configViewModel.Removed += mainViewModel.OnNodeRemoved;
            configViewModel.Updated += mainViewModel.OnNodeUpdated;
            configViewModel.Added += mainViewModel.OnNodeAdded;

            win.Content = new ConfigPanel()
            {
                DataContext = configViewModel
            };

            win.Width = 600;
            win.Height = 300;
            win.Show();
        }
#else
        private void ShowConfigDialog(MainViewModel mainViewModel)
        {
            var win = new Window();
            win.Title = "Configure";
            win.Owner = App.Current.MainWindow;

            var configViewModel = new ConfigViewModel(mainViewModel.AgentConfig);
            configViewModel.Removed += mainViewModel.OnNodeRemoved;
            configViewModel.Updated += mainViewModel.OnNodeUpdated;
            configViewModel.Added += mainViewModel.OnNodeAdded;

            win.Content = new ConfigPanel()
            {
                DataContext = configViewModel
            };

            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            win.WindowStyle = WindowStyle.SingleBorderWindow;
            win.ResizeMode = ResizeMode.NoResize;
            win.Width = 600;
            win.Height = 300;
            win.ShowDialog();
        }
#endif
    }
}
