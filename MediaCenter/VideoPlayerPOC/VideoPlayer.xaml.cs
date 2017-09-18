using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VideoPlayerPOC
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private int _milisecondPerSliderTick = 100;
        private DispatcherTimer _timer;
        private bool _isDragging = false;
        private bool _sliderlengthSet = false;
        public VideoPlayer()
        {
            InitializeComponent();
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += new EventHandler(TimerTick);
        }

        private void TimerTick(object sender, EventArgs eventArgs)
        {
            if (!_sliderlengthSet && MediaElement.NaturalDuration.HasTimeSpan)
            {
                SeekSlider.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds / _milisecondPerSliderTick;
                _sliderlengthSet = true;
            }
            if (!_isDragging)
            {
                SeekSlider.Value = MediaElement.Position.TotalMilliseconds/_milisecondPerSliderTick;
            }
        }
        
        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            SeekSlider.Minimum = 0;
            SeekSlider.Value = 0;

            if (MediaElement.NaturalDuration.HasTimeSpan)
            {
                SeekSlider.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds / _milisecondPerSliderTick;
                _sliderlengthSet = true;
            }
            else
            {
                SeekSlider.Maximum = 0;
                _sliderlengthSet = false;
            }
            
            _timer.Start();
        }

        private void SeekSlider_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        private void SeekSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            MediaElement.Position = TimeSpan.FromMilliseconds(SeekSlider.Value*_milisecondPerSliderTick);
            _isDragging = false;
        }
    }
}

