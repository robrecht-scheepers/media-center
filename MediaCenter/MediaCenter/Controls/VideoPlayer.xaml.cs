using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaCenter.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private enum PlayState { Playing, Paused, Stopped }

        private int _milisecondPerSliderTick = 200;
        private DispatcherTimer _timer;
        private bool _isDragging = false;
        private bool _sliderlengthSet = false;
        
        public VideoPlayer()
        {
            InitializeComponent();
            _playState = PlayState.Stopped;
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += new EventHandler(TimerTick);
        }

        public bool StartOnLoad { get { return (bool)GetValue(StartOnLoadProperty); } set { SetValue(StartOnLoadProperty, value); } }
        public static readonly DependencyProperty StartOnLoadProperty = DependencyProperty.Register("StartOnLoad", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(false, StartOnLoadChanged));

        public Uri VideoUri { get { return (Uri)GetValue(VideoUriProperty); } set { SetValue(VideoUriProperty, value); } }
        public static readonly DependencyProperty VideoUriProperty = DependencyProperty.Register("VideoUri", typeof(System.Uri), typeof(VideoPlayer), new PropertyMetadata(default(Uri), VideoUriChanged));

        public int Rotation { get { return (int)GetValue(RotationProperty); } set { SetValue(RotationProperty, value); } }
        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(int), typeof(VideoPlayer), new PropertyMetadata(0, RotationChanged));
        
        private static void StartOnLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            if(me == null) return;

            if (me.StartOnLoad)
                me.MediaElement.LoadedBehavior = MediaState.Play;
            else
                me.MediaElement.LoadedBehavior = MediaState.Manual;
        }

        private static void RotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            if (me == null) return;

            me.Rotate((int)e.NewValue);
        }

        private static void VideoUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as VideoPlayer;
            if (me == null) return;

            me.LoadVideo((Uri)e.NewValue);
        }

        public void LoadVideo(Uri videoUri)
        {
            if(MediaElement.Source == videoUri)
                return;

            Stop();
            MediaElement.Source = videoUri;
        }

        public void Rotate(int angle)
        {
            if (angle == 0)
            {
                MediaElement.LayoutTransform = null;
            }
            else
            {
                MediaElement.LayoutTransform = new RotateTransform(angle);
            }
        }

        #region SeekBar
        private void TimerTick(object sender, EventArgs eventArgs)
        {
            if (!_sliderlengthSet && MediaElement.NaturalDuration.HasTimeSpan)
            {
                SeekSlider.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds / _milisecondPerSliderTick;
                TotalTime.Text = MediaElement.NaturalDuration.TimeSpan.ToString("mm\\:ss");
                _sliderlengthSet = true;
            }

            if (!_isDragging)
            {
                SeekSlider.Value = MediaElement.Position.TotalMilliseconds / _milisecondPerSliderTick;
            }
            CurrentTime.Text = MediaElement.Position.ToString("mm\\:ss");
        }

        private void MediaElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            SeekSlider.Minimum = 0;
            SeekSlider.Value = 0;

            if (MediaElement.NaturalDuration.HasTimeSpan)
            {
                SeekSlider.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds / _milisecondPerSliderTick;
                CurrentTime.Text = "00:00";
                TotalTime.Text = MediaElement.NaturalDuration.TimeSpan.ToString("mm\\:ss");
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
            MediaElement.Position = TimeSpan.FromMilliseconds(SeekSlider.Value * _milisecondPerSliderTick);
            _isDragging = false;
        }
        #endregion

        #region Control

        private PlayState _playState;

        private void ApplyPlayState(PlayState state)
        {
            if (_playState == state)
                return;

            switch (state)
            {
                case PlayState.Stopped:
                    _playState = PlayState.Stopped;
                    MediaElement.Stop();
                    break;
                case PlayState.Paused:
                    _playState = PlayState.Paused;
                    MediaElement.Pause();
                    break;
                case PlayState.Playing:
                    _playState = PlayState.Playing;
                    MediaElement.Play();
                    break;
            }
        }

        private void Stop()
        {
            ApplyPlayState(PlayState.Stopped);
        }

        private void Pause()
        {
            ApplyPlayState(PlayState.Paused);
        }

        private void Play()
        {
            ApplyPlayState(PlayState.Playing);
        }

        private void PlayPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (_playState == PlayState.Playing)
                Pause();
            else
                Play();
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        #endregion

        private void SeekSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!_isDragging)
                MediaElement.Position = TimeSpan.FromMilliseconds(SeekSlider.Value * _milisecondPerSliderTick);
        }
    }
}
