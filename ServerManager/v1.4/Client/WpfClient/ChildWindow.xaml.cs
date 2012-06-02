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
using System.Windows.Shapes;

namespace SuperSocket.Management.Client
{
    /// <summary>
    /// Interaction logic for ChildWindow.xaml
    /// </summary>
    public partial class ChildWindow : Window
    {
        public ChildWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ChildWindow_Loaded);
        }

        void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowTitle.Text = this.Title;
        }

        private Control m_ChildContent;

        public Control ChildContent
        {
            set
            {
                if (m_ChildContent != null)
                    throw new Exception("Cannot set child content twice");

                m_ChildContent = value;
                m_ChildContent.SetValue(Grid.RowProperty, 1);
                m_ChildContent.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                m_ChildContent.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Stretch);
                LayoutRoot.Children.Add(m_ChildContent);
            }

            get { return m_ChildContent; }
        }
    }
}
