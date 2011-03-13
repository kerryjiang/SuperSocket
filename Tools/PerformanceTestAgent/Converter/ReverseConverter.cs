using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace PerformanceTestAgent.Converter
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class ReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            return !boolValue;
        }
    }
}
