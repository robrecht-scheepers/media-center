using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;

namespace MediaCenter.Sessions.Query
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
            return Invert ? mode == ViewMode.List : mode == ViewMode.Detail;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = (bool) value;

            if(Invert)
                return input ? ViewMode.List : ViewMode.Detail;

            return input ? ViewMode.Detail : ViewMode.List;
        }

        public bool Invert { get; set; }
    }
}
