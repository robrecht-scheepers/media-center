using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MediaCenter.Media;

namespace MediaCenter.WPF.Converters
{
    public class PlayStateVisibilityConverter : IValueConverter
    {
        public PlayStateVisibilityConverter()
        {
            // set default values
            PlayingVisibility = Visibility.Visible;
            NotPlayingVisibility = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NotPlayingVisibility;

            var state = (PlayState)value;
            return state == PlayState.Playing ? PlayingVisibility : NotPlayingVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Visibility PlayingVisibility { get; set; }
        public Visibility NotPlayingVisibility { get; set; }
    }
}
