using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MediaCenter.WPF.Controls
{
    public class SingleImageToggleButton : ToggleButton
    {
        static SingleImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SingleImageToggleButton), new FrameworkPropertyMetadata(typeof(SingleImageToggleButton)));
        }

        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(string), typeof(SingleImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));


        public SolidColorBrush HoverBackground
        {
            get { return (SolidColorBrush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(SolidColorBrush), typeof(SingleImageToggleButton), new PropertyMetadata(Application.Current.FindResource("C1")));

        public SolidColorBrush CheckedBackground
        {
            get { return (SolidColorBrush)GetValue(CheckedBackgroundProperty); }
            set { SetValue(CheckedBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HoverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register("CheckedBackground", typeof(SolidColorBrush), typeof(SingleImageToggleButton), new PropertyMetadata(Application.Current.FindResource("C1")));

        

        private static void ImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Application.GetResourceStream(new Uri("pack://application:,,," + (string)e.NewValue));
        }
    }
}
