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
    public class ToolWindoStateToVisibilityConverter : IValueConverter
    {
        public QueryToolWindowState? TargetWindowState { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is QueryToolWindowState windowState))
                return Visibility.Visible;

            switch (windowState)
            {
                case QueryToolWindowState.Hidden:
                    return Visibility.Collapsed;
                case QueryToolWindowState.Filters:
                    return (TargetWindowState == null || TargetWindowState.Value == QueryToolWindowState.Filters) ? Visibility.Visible : Visibility.Collapsed;
                case QueryToolWindowState.Properties:
                    return (TargetWindowState == null || TargetWindowState.Value == QueryToolWindowState.Properties) ? Visibility.Visible : Visibility.Collapsed;
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
