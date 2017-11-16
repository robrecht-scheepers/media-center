using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaCenter.WPF.Controls
{
    public class ColumnToggleButton : ImageToggleButton
    {
        static ColumnToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnToggleButton), new FrameworkPropertyMetadata(typeof(ColumnToggleButton)));
        }
    }
}
