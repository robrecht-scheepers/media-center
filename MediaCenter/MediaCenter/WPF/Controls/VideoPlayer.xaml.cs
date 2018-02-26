using MediaCenter.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaCenter.WPF.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private int _milisecondPerSliderTick = 200;
        private DispatcherTimer _timer;
        private bool _isDragging = false;
        private bool _sliderlengthSet = false;
        
        public VideoPlayer()
        {
            InitializeComponent();
            PlayState = PlayState.Stopped;
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += new EventHandler(TimerTick);
        }

        public bool HideControls
        {
            get { return (bool)GetValue(HideControlsProperty); }
            set { SetValue(HideControlsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HideControls.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideControlsProperty =
            DependencyProperty.Register("HideControls", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(false));
        

        public bool StartOnLoad { get { return (bool)GetValue(StartOnLoadProperty); } set { SetValue(StartOnLoadProperty, value); } }
        public static readonly DependencyProperty StartOnLoadProperty = DependencyProperty.Register("StartOnLoad", typeof(bool), typeof(VideoPlayer), new PropertyMetadata(false));

        public Uri VideoUri { get { return (Uri)GetValue(VideoUriProperty); } set { SetValue(VideoUriProperty, value); } }
        public static readonly DependencyProperty VideoUriProperty = DependencyProperty.Register("VideoUri", typeof(System.Uri), typeof(VideoPlayer), new PropertyMetadata(default(Uri), VideoUriChanged));

        public int Rotation { get { return (int)GetValue(RotationProperty); } set { SetValue(RotationProperty, value); } }
        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(int), typeof(VideoPlayer), new PropertyMetadata(0, RotationChanged));
        
        public PlayState PlayState{ get { return (PlayState)GetValue(PlayStateProperty); } set { SetValue(PlayStateProperty, value); } }
        public static readonly DependencyProperty PlayStateProperty = DependencyProperty.Register("PlayState", typeof(PlayState), typeof(VideoPlayer), new PropertyMetadata(PlayState.Stopped, OnPlayStateChanged));

        private static void OnPlayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (VideoPlayer) d;

            me.ApplyPlayStateChange((PlayState)e.OldValue, (PlayState)e.NewValue);
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

            if(StartOnLoad)
                Play(); 
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
        
        private void ApplyPlayStateChange(PlayState oldPlayState, PlayState newPlayState)
        { 
            if (oldPlayState == newPlayState)
                return;

            switch (newPlayState)
            {
                case PlayState.Stopped:
                    MediaElement.Stop();
                    break;
                case PlayState.Paused:
                    MediaElement.Pause();
                    break;
                case PlayState.Playing:
                    MediaElement.Play();
                    break;
                case PlayState.Finished:
                    // do nothing, the media element has stopped by itself. 
                    // The state is set to finished, notify anyone who needs to know this (for instance the slide show)
                    break;
            }
        }

        private void Stop()
        {
            PlayState = PlayState.Stopped;
        }

        private void Pause()
        {
            PlayState = PlayState.Paused;
        }

        private void Play()
        {
            PlayState = PlayState.Playing;
        }

        private void PlayPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlayState == PlayState.Playing)
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
            PlayState = PlayState.Finished; 
        }
        #endregion

        private void SeekSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!_isDragging)
                MediaElement.Position = TimeSpan.FromMilliseconds(SeekSlider.Value * _milisecondPerSliderTick);
        }
    }
}
