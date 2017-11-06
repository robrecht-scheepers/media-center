﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace MediaCenter.Controls
{
    public class ImageToggleButton : ToggleButton
    {
        static ImageToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageToggleButton), new FrameworkPropertyMetadata(typeof(ImageToggleButton)));
        }

        public string CheckedImage
        {
            get { return (string)GetValue(CheckedImageProperty); }
            set { SetValue(CheckedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register("CheckedImage", typeof(string), typeof(ImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));

        public string UncheckedImage
        {
            get { return (string)GetValue(UncheckedImageProperty); }
            set { SetValue(UncheckedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UncheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UncheckedImageProperty =
            DependencyProperty.Register("UncheckedImage", typeof(string), typeof(ImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));

        public string NullImage
        {
            get { return (string)GetValue(NullImageProperty); }
            set { SetValue(NullImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NullImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NullImageProperty =
            DependencyProperty.Register("NullImage", typeof(string), typeof(ImageToggleButton), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, ImageSourceChanged));


        private static void ImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Application.GetResourceStream(new Uri("pack://application:,,," + (string)e.NewValue));
        }
    }
}
