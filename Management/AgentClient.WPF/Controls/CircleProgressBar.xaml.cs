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
using System.Windows.Media.Animation;

namespace SuperSocket.ServerManager.Client.Controls
{
    /// <summary>
    /// Interaction logic for CircleProgressBar.xaml
    /// </summary>
    public partial class CircleProgressBar : UserControl
    {
        public static readonly DependencyProperty ProgressTextProperty = DependencyProperty.Register(
              "ProgressText",
              typeof(string),
              typeof(CircleProgressBar),
              new PropertyMetadata(ProgressTextChangedCallback)
            );

        static void ProgressTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bar = d as CircleProgressBar;
            bar.ProgressTextLabel.Text = e.NewValue != null ? e.NewValue.ToString() : string.Empty;
        }

        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            set
            {
                SetValue(ProgressTextProperty, value);
            }
        }

        public CircleProgressBar()
        {
            InitializeComponent();
        }
    }
}
