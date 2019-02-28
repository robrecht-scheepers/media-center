using System.Windows;
using System.Windows.Controls;

namespace MediaCenter.WPF.Controls.AirControl
{
    /// <summary>
    /// Interaction logic for AirControl.xaml
    /// </summary>
    public partial class AirControl : UserControl
    {
        private Alpha alpha;
        private UIElement back;

        public UIElement Back
        {
            get
            {
                return back;
            }
            set
            {
                back = value;
                contentGrid.Children.Clear();
                contentGrid.Children.Add(back);
            }
        }

        private UIElement front;

        public UIElement Front
        {
            get
            {
                return front;
            }
            set
            {
                front = value;
                alpha.Content = front;
            }
        }


        public AirControl()
        {
            InitializeComponent();
            alpha = new Alpha(this);
        }
    }
}
