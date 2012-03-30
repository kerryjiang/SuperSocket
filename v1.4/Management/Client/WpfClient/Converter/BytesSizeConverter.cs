using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SuperSocket.Management.Client.Converter
{
    public class BytesSizeConverter : IValueConverter
    {
        private BytesSizeFormatProvider m_FormatProvider = new BytesSizeFormatProvider();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return String.Format(m_FormatProvider, "{0:fs}", (double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
