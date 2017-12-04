﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaCenter.WPF.Controls
{
    public class TextButton : Button
    {
        static TextButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextButton),
                new FrameworkPropertyMetadata(typeof(TextButton)));
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextButton), new PropertyMetadata(""));


    }
}
