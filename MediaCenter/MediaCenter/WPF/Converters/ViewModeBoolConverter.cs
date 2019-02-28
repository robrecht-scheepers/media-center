using System;
using System.Globalization;
using System.Windows.Data;
using MediaCenter.Sessions.Query;

namespace MediaCenter.WPF.Converters
{
    public class ViewModeBoolConverter : IValueConverter
    {
        public ViewModeBoolConverter()
        {
            Invert = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = (ViewMode) value;
            return Invert ? mode == ViewMode.Grid : mode == ViewMode.Detail;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = (bool) value;

            if(Invert)
                return input ? ViewMode.Grid : ViewMode.Detail;

            return input ? ViewMode.Detail : ViewMode.Grid;
        }

        public bool Invert { get; set; }
    }
}
