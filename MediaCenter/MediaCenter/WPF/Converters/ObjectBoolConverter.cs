using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaCenter.WPF.Converters
{
    public class ObjectBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Invert ? value == null : value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool) value)
                return null;
            else
                throw new ArgumentException("ConvertBack is only possible for false value. Cannot create an object of unknown type."); 
        }

        public bool Invert { get; set; }
    }
}
