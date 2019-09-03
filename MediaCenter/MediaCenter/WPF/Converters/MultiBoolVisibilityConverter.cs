using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaCenter.WPF.Converters
{
    public class MultiBoolVisibilityConverter : IMultiValueConverter
    {
        public enum Operator { And, Or}

        public Visibility TrueVisibility { get; set; }
        public Visibility FalseVisibilty { get; set; }
        public Operator LogicalOperator { get; set; }
        

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValues = values.OfType<bool>().ToList();
            if (!boolValues.Any())
                return FalseVisibilty;

            foreach (var b in boolValues)
            {
                if (b && LogicalOperator == Operator.Or)
                    return TrueVisibility;
                if (!b && LogicalOperator == Operator.And)
                    return FalseVisibilty;
            }
            return LogicalOperator == Operator.And ? TrueVisibility : FalseVisibilty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
