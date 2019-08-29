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
    public class ToolWindoStateToBoolConverter : IValueConverter
    {
        public QueryToolWindowState? TargetWindowState { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is QueryToolWindowState windowState))
                return true;

            switch (windowState)
            {
                case QueryToolWindowState.Hidden:
                    return false;
                case QueryToolWindowState.Filters:
                case QueryToolWindowState.Properties:
                    return true;
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
