using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaCenter.WPF.Controls
{
    /// <summary>
    /// Interaction logic for CropImage.xaml
    /// </summary>
    public partial class CropImage : UserControl
    {
        public CropImage()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ImageContentProperty = DependencyProperty.Register(
            "ImageContent", typeof(byte[]), typeof(CropImage), new PropertyMetadata(null, ImageContentChanged));
        public byte[] ImageContent
        {
            get => (byte[]) GetValue(ImageContentProperty);
            set => SetValue(ImageContentProperty, value);
        }
        private static void ImageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static readonly DependencyProperty FoVPositionXProperty = DependencyProperty.Register(
            "FoVPositionX", typeof(int), typeof(CropImage), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty FieldOfViewProperty = DependencyProperty.Register(
            "FieldOfView", typeof(Rectangle), typeof(CropImage), new PropertyMetadata(null, FieldOfViewChanged));

        private static void FieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // reset
            throw new NotImplementedException();
        }

        public Rectangle FieldOfView
        {
            get { return (Rectangle) GetValue(FieldOfViewProperty); }
            set { SetValue(FieldOfViewProperty, value); }
        }

    }
}
