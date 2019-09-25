using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using MediaCenter.Media;

namespace MediaCenter.WPF.Converters
{
    public class MediaItemStatusVisibilityConverter : IValueConverter
    {
        public MediaItemStatus MatchStatus { get; set; }
        public Visibility MatchVisibility { get; set; }
        public Visibility NoMatchVisibility { get; set; }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MediaItemStatus status))
                return Visibility.Visible;

            return (status == MatchStatus) ? MatchVisibility : NoMatchVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
