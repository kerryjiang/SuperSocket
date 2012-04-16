using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SuperSocket.Management.Client.ViewModel;

namespace SuperSocket.Management.Client.Converter
{
    public class RowTemplateConverter : IValueConverter
    {
        public ControlTemplate NormalTemplate { get; set; }

        public ControlTemplate LoadingTemplate { get; set; }

        public ControlTemplate FaultTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is LoadingInstanceViewModel)
                return LoadingTemplate;
            else if (value is FaultInstanceViewModel)
                return FaultTemplate;
            else
                return NormalTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
