using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaCenter.WPF
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public BoolVisibilityConverter()
        {
            // set default values
            TrueVisibility = Visibility.Visible;
            FalseVisibility = Visibility.Collapsed;
            NullVisibility = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NullVisibility;

            return (bool)value ? TrueVisibility : FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Visibility TrueVisibility { get; set; }
        public Visibility FalseVisibility { get; set; }
        public Visibility NullVisibility { get; set; }
    }
}
