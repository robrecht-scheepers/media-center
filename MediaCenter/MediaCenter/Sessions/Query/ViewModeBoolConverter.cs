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
            var mode = (QuerySessionViewModel.ViewMode) value;
            return Invert ? mode == QuerySessionViewModel.ViewMode.List : mode == QuerySessionViewModel.ViewMode.Detail;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = (bool) value;

            if(Invert)
                return input ? QuerySessionViewModel.ViewMode.List : QuerySessionViewModel.ViewMode.Detail;

            return input ? QuerySessionViewModel.ViewMode.Detail : QuerySessionViewModel.ViewMode.List;
        }

        public bool Invert { get; set; }
    }
}
