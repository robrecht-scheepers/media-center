using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaCenter.Sessions.Query
{
    public class ViewModeToVisibilityConverter : IValueConverter
    {
        public Visibility VisibilityWhenGrid { get; set; }
        public Visibility VisibilityWhenDetail { get; set; }
        public Visibility VisibilityWhenSlideshow { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ViewMode viewMode))
                return Visibility.Collapsed;

            switch (viewMode)
            {
                case ViewMode.Grid:
                    return VisibilityWhenGrid;
                case ViewMode.Detail:
                    return VisibilityWhenDetail;
                case ViewMode.SlideShow:
                    return VisibilityWhenSlideshow;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
