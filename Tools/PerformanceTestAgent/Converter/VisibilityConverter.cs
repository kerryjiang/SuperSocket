using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace PerformanceTestAgent.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool whetherDisplay = (bool)value;
            bool whetherReverse = false;

            if (parameter != null)
            {
                if (parameter is bool)
                {
                    if (!(bool)parameter)
                        whetherReverse = true;
                }
                else if (parameter is string)
                {
                    bool boolParam;
                    if(bool.TryParse(parameter.ToString(), out boolParam))
                        if (!boolParam)
                            whetherReverse = true;
                }
            }

            if (whetherReverse)
                whetherDisplay = !whetherDisplay;

            return whetherDisplay ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            bool whetherReverse = false;

            if (parameter != null)
            {
                if (parameter is bool)
                {
                    if (!(bool)parameter)
                        whetherReverse = true;
                }
                else if (parameter is string)
                {
                    bool boolParam;
                    if (bool.TryParse(parameter.ToString(), out boolParam))
                        if (!boolParam)
                            whetherReverse = true;
                }
            }

            return (visibility == Visibility.Visible) != whetherReverse ? true : false;
        }
    }
}
