using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaCenter.Sessions.Query
{
    public class ViewModeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ViewMode viewMode))
                return false;

            switch (viewMode)
            {
                case ViewMode.Grid:
                    return false;
                case ViewMode.Detail:
                    return true;
                case ViewMode.SlideShow:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ViewMode.SlideShow;

            var isChecked = (bool?) value;

            return isChecked.Value ? ViewMode.Detail : ViewMode.Grid;
        }
    }
}
