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
using PerformanceTestAgent.ViewModel;
using System.Threading;

namespace PerformanceTestAgent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int m_TotalMessages = 0;

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            viewModel.ContentAppend += new EventHandler<ContentAppendEventArgs>(viewModel_ContentAppend);
            this.DataContext = viewModel;
        }

        void viewModel_ContentAppend(object sender, ContentAppendEventArgs e)
        {
            bool clearFirst = false;

            if (m_TotalMessages > 2000)
            {
                clearFirst = true;
            }
                
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (clearFirst)
                {
                    Interlocked.Exchange(ref m_TotalMessages, 0);
                    tbDashBoard.Clear();
                }
                tbDashBoard.AppendText(e.Content);
            }));

            Interlocked.Increment(ref m_TotalMessages);
        }
    }
}
