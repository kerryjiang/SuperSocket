using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SuperSocket.Management.Client.ViewModel;

namespace SuperSocket.Management.Client.Converter
{
    public class ViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var viewModelSource = value as Dictionary<string, IViewModelAccessor>;

            IViewModelAccessor viewModelAccessor;

            if (!viewModelSource.TryGetValue(parameter.ToString(), out viewModelAccessor))
                throw new ArgumentException("parameter");

            return viewModelAccessor.GetViewModel();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
