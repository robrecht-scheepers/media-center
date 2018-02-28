using System.Windows;
using System.Windows.Controls;
using MediaCenter.WPF.Controls;

namespace MediaCenter.Media
{
    /// <summary>
    /// Interaction logic for VideoItemView.xaml
    /// </summary>
    public partial class VideoItemView : UserControl
    {
        public VideoItemView()
        {
            InitializeComponent();
        }

        public bool HideControls
        {
            get { return (bool)GetValue(HideControlsProperty); }
            set { SetValue(HideControlsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HideControls.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideControlsProperty =
            DependencyProperty.Register("HideControls", typeof(bool), typeof(VideoItemView), new PropertyMetadata(false));

    }
}
