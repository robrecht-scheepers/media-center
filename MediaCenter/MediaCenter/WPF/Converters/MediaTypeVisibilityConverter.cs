using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MediaCenter.Media;

namespace MediaCenter.WPF.Converters
{
    public class MediaTypeVisibilityConverter : IValueConverter
    {
        public MediaTypeVisibilityConverter()
        {
            // set default values
            ImageVisibility = Visibility.Visible;
            VideoVisibility = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return Visibility.Collapsed;
                case MediaType mediaType:
                    return mediaType == MediaType.Image ? ImageVisibility : VideoVisibility;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Visibility ImageVisibility { get; set; }
        public Visibility VideoVisibility { get; set; }
    }
}
