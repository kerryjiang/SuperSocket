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

namespace SuperSocket.ServerManager.Client
{
    /// <summary>
    /// Interaction logic for NodeIndo.xaml
    /// </summary>
    public partial class NodeInfo : UserControl
    {
        public NodeInfo()
        {
            InitializeComponent();
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as NodeMasterViewModel;
            if (viewModel != null)
                viewModel.NodeDetailDataContextChanged(sender, e);
        }
    }
}
