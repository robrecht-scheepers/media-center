using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaCenter.WPF.Controls
{
    public class HideableColumnDefinition : ColumnDefinition
    {
        static HideableColumnDefinition()
        {
            ColumnDefinition.WidthProperty.OverrideMetadata(
                typeof(HideableColumnDefinition), 
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null, new CoerceValueCallback(CoerceWidth)));
        }

        private static object CoerceWidth(DependencyObject d, object baseValue)
        {
            return ((HideableColumnDefinition) d).Visible ? baseValue : new GridLength(0);
        }

        public Boolean Visible
        {
            get { return (Boolean)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibleProperty =
            DependencyProperty.Register("Visible", typeof(Boolean), typeof(HideableColumnDefinition), new PropertyMetadata(true, OnVisibleChanged));



        private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ColumnDefinition.WidthProperty);
        }
    }
}
