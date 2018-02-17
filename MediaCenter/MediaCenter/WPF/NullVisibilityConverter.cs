using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaCenter.WPF
{
    public class NullVisibilityConverter : IValueConverter
    {
        public NullVisibilityConverter()
        {
            NotNullVisibility = Visibility.Visible;
            NullVisibility = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullVisibility : NotNullVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Visibility NullVisibility { get; set; }
        public Visibility NotNullVisibility { get; set; }
    }
}
