using System.Windows;

namespace MediaCenter.Sessions.Slideshow
{
    /// <summary>
    /// Interaction logic for SlideShowWindow.xaml
    /// </summary>
    public partial class SlideShowWindow : Window
    {
        public SlideShowWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
