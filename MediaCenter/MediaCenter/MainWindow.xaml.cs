using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MediaCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MaximizeWindow();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Adjust_OnClick(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                MaximizeWindow();
            }
        }

        private void MaximizeWindow()
        {
            var size = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle).Bounds;
            this.MaxWidth = size.Width;
            this.MaxHeight = size.Height;
            this.WindowState = WindowState.Maximized;
        }

        private void TitleBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }
    }
}
