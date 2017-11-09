using System;
using System.Windows;
using System.Windows.Controls;

namespace MediaCenter.Controls
{
    public class HideableRowDefinition : RowDefinition
    {
        static HideableRowDefinition()
        {
            HeightProperty.OverrideMetadata(typeof(HideableRowDefinition), 
                new FrameworkPropertyMetadata(new GridLength(1,GridUnitType.Star),null, new CoerceValueCallback(CoerceHeight)));
        }

        private static object CoerceHeight(DependencyObject d, object baseValue)
        {
            return ((HideableRowDefinition)d).Visible ? baseValue : new GridLength(0);
        }

        public Boolean Visible
        {
            get { return (Boolean)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibleProperty =
            DependencyProperty.Register("Visible", typeof(Boolean), typeof(HideableRowDefinition), new PropertyMetadata(true, OnVisibleChanged));

        private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(RowDefinition.HeightProperty);
        }
    }
}
